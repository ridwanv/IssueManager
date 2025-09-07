// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Hangfire;

namespace CleanArchitecture.Blazor.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job for processing completed conversations and generating AI insights
/// </summary>
public class ProcessCompletedConversationsJob
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IConversationAnalysisService _analysisService;
    private readonly ILogger<ProcessCompletedConversationsJob> _logger;
    private readonly IApplicationHubWrapper? _hubWrapper;

    public ProcessCompletedConversationsJob(
        IApplicationDbContextFactory dbContextFactory,
        IConversationAnalysisService analysisService,
        ILogger<ProcessCompletedConversationsJob> logger,
        IApplicationHubWrapper? hubWrapper = null)
    {
        _dbContextFactory = dbContextFactory;
        _analysisService = analysisService;
        _logger = logger;
        _hubWrapper = hubWrapper;
    }

    /// <summary>
    /// Processes all unprocessed completed conversations
    /// </summary>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 120, 300 })]
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var processedCount = 0;
        var errorCount = 0;
        
        _logger.LogInformation("Starting ProcessCompletedConversationsJob execution");

        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
            
            // Find completed conversations without insights
            var unprocessedConversations = await GetUnprocessedCompletedConversationsAsync(db, cancellationToken);
            
            if (!unprocessedConversations.Any())
            {
                _logger.LogInformation("No unprocessed completed conversations found");
                return;
            }

            _logger.LogInformation("Found {Count} unprocessed completed conversations", unprocessedConversations.Count);

            // Process conversations in batches to avoid overwhelming the AI service
            const int batchSize = 5;
            var batches = unprocessedConversations.Chunk(batchSize);

            foreach (var batch in batches)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Job cancellation requested, stopping processing");
                    break;
                }

                var batchResults = await ProcessConversationBatchAsync(batch, cancellationToken);
                
                foreach (var (conversation, result) in batchResults)
                {
                    try
                    {
                        await SaveConversationInsightAsync(db, conversation, result, cancellationToken);
                        processedCount++;
                        
                        // Note: SignalR notification for insights can be added via custom hub method if needed

                        _logger.LogDebug("Successfully processed conversation {ConversationId}", conversation.Id);
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        _logger.LogError(ex, "Failed to save insights for conversation {ConversationId}", conversation.Id);
                    }
                }

                // Small delay between batches to avoid overwhelming the system
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }

            _logger.LogInformation("ProcessCompletedConversationsJob completed in {Duration}ms. Processed: {Processed}, Errors: {Errors}",
                stopwatch.ElapsedMilliseconds, processedCount, errorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProcessCompletedConversationsJob failed after {Duration}ms", stopwatch.ElapsedMilliseconds);
            throw; // Re-throw to trigger Hangfire retry mechanism
        }
    }

    /// <summary>
    /// Processes a single conversation (for manual or retry scenarios)
    /// </summary>
    [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 60, 180 })]
    public async Task ProcessSingleConversationAsync(int conversationId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing single conversation {ConversationId}", conversationId);

        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
            
            var conversation = await db.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.Status == ConversationStatus.Completed, cancellationToken);

            if (conversation == null)
            {
                _logger.LogWarning("Conversation {ConversationId} not found or not completed", conversationId);
                return;
            }

            // Check if already processed
            var existingInsight = await db.ConversationInsights
                .AnyAsync(ci => ci.ConversationId == conversationId, cancellationToken);

            if (existingInsight)
            {
                _logger.LogInformation("Conversation {ConversationId} already has insights", conversationId);
                return;
            }

            var analysisResult = await _analysisService.AnalyzeConversationAsync(conversation, cancellationToken);
            await SaveConversationInsightAsync(db, conversation, analysisResult, cancellationToken);

            _logger.LogInformation("Successfully processed single conversation {ConversationId}", conversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process single conversation {ConversationId}", conversationId);
            throw;
        }
    }

    private async Task<List<Conversation>> GetUnprocessedCompletedConversationsAsync(IApplicationDbContext db, CancellationToken cancellationToken)
    {
        // Find completed conversations that don't have insights yet
        var conversations = await db.Conversations
            .Include(c => c.Messages)
            .Where(c => c.Status == ConversationStatus.Completed && 
                       c.CompletedAt != null &&
                       !db.ConversationInsights.Any(ci => ci.ConversationId == c.Id))
            .OrderBy(c => c.CompletedAt) // Process oldest first
            .Take(50) // Limit to prevent overwhelming the system
            .ToListAsync(cancellationToken);

        return conversations;
    }

    private async Task<List<(Conversation conversation, ConversationAnalysisResult result)>> ProcessConversationBatchAsync(
        IEnumerable<Conversation> conversations, 
        CancellationToken cancellationToken)
    {
        var results = new List<(Conversation, ConversationAnalysisResult)>();
        
        foreach (var conversation in conversations)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                var analysisResult = await _analysisService.AnalyzeConversationAsync(conversation, cancellationToken);
                results.Add((conversation, analysisResult));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to analyze conversation {ConversationId}", conversation.Id);
                
                // Create a failed result to still store something
                var failedResult = new ConversationAnalysisResult
                {
                    SentimentScore = 0,
                    SentimentLabel = "Analysis Failed",
                    ProcessingModel = "error",
                    ProcessingDuration = TimeSpan.Zero,
                    Warnings = new List<string> { $"Analysis failed: {ex.Message}" }
                };
                results.Add((conversation, failedResult));
            }
        }

        return results;
    }

    private async Task SaveConversationInsightAsync(
        IApplicationDbContext db, 
        Conversation conversation, 
        ConversationAnalysisResult result, 
        CancellationToken cancellationToken)
    {
        var insight = new ConversationInsight
        {
            ConversationId = conversation.Id,
            SentimentScore = result.SentimentScore,
            SentimentLabel = result.SentimentLabel,
            KeyThemes = JsonSerializer.Serialize(result.KeyThemes),
            ResolutionSuccess = result.ResolutionSuccess,
            CustomerSatisfactionIndicators = JsonSerializer.Serialize(result.CustomerSatisfactionIndicators),
            Recommendations = JsonSerializer.Serialize(result.Recommendations),
            ProcessingModel = result.ProcessingModel,
            ProcessedAt = DateTime.UtcNow,
            ProcessingDuration = result.ProcessingDuration,
            TenantId = conversation.TenantId
        };

        db.ConversationInsights.Add(insight);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Schedules the recurring job (called during application startup)
    /// </summary>
    public static void ScheduleRecurringJob()
    {
        // Run every hour
        RecurringJob.AddOrUpdate<ProcessCompletedConversationsJob>(
            "process-completed-conversations",
            job => job.ExecuteAsync(CancellationToken.None),
            Cron.Hourly);
    }

    /// <summary>
    /// Schedules immediate execution for testing/manual trigger
    /// </summary>
    public static string ScheduleImmediateJob()
    {
        return BackgroundJob.Enqueue<ProcessCompletedConversationsJob>(
            job => job.ExecuteAsync(CancellationToken.None));
    }
}