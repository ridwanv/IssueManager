// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Commands.CreateConversationInsight;

/// <summary>
/// Handler for creating conversation insights from AI analysis results
/// </summary>
public class CreateConversationInsightCommandHandler : IRequestHandler<CreateConversationInsightCommand, Result<int>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly ILogger<CreateConversationInsightCommandHandler> _logger;

    public CreateConversationInsightCommandHandler(
        IApplicationDbContextFactory dbContextFactory,
        ILogger<CreateConversationInsightCommandHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateConversationInsightCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

            // Check if conversation exists and belongs to the same tenant
            var conversation = await db.Conversations
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

            if (conversation == null)
            {
                _logger.LogWarning("Conversation {ConversationId} not found", request.ConversationId);
                return await Result<int>.FailureAsync($"Conversation with ID {request.ConversationId} not found");
            }

            // Check if insight already exists for this conversation
            var existingInsight = await db.ConversationInsights
                .FirstOrDefaultAsync(ci => ci.ConversationId == request.ConversationId, cancellationToken);

            if (existingInsight != null)
            {
                _logger.LogWarning("Insight already exists for conversation {ConversationId}", request.ConversationId);
                return await Result<int>.FailureAsync($"Insight already exists for conversation {request.ConversationId}");
            }

            // Create new conversation insight
            var insight = new ConversationInsight
            {
                ConversationId = request.ConversationId,
                SentimentScore = request.SentimentScore,
                SentimentLabel = request.SentimentLabel,
                KeyThemes = JsonSerializer.Serialize(request.KeyThemes),
                ResolutionSuccess = request.ResolutionSuccess,
                CustomerSatisfactionIndicators = JsonSerializer.Serialize(request.CustomerSatisfactionIndicators),
                Recommendations = JsonSerializer.Serialize(request.Recommendations),
                ProcessingModel = request.ProcessingModel,
                ProcessedAt = request.ProcessedAt,
                ProcessingDuration = request.ProcessingDuration,
                TenantId = conversation.TenantId // Ensure tenant isolation
            };

            db.ConversationInsights.Add(insight);
            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created conversation insight {InsightId} for conversation {ConversationId} with sentiment {SentimentLabel}",
                insight.Id, request.ConversationId, request.SentimentLabel);

            return await Result<int>.SuccessAsync(insight.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create conversation insight for conversation {ConversationId}", request.ConversationId);
            return await Result<int>.FailureAsync($"Failed to create conversation insight: {ex.Message}");
        }
    }
}