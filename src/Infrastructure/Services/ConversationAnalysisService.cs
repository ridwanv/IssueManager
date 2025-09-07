// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Diagnostics;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Infrastructure.Services;

public class ConversationAnalysisService : IConversationAnalysisService
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly ILogger<ConversationAnalysisService> _logger;
    private readonly Kernel _kernel;
    private readonly string _deploymentName;
    private readonly ChatHistory _systemInstructions;

    public ConversationAnalysisService(
        IApplicationDbContextFactory dbContextFactory,
        ILogger<ConversationAnalysisService> logger,
        IConfiguration configuration)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        
        var endpoint = configuration["AZURE_OPENAI_API_ENDPOINT"] ?? throw new InvalidOperationException("AZURE_OPENAI_API_ENDPOINT configuration is required");
        var apiKey = configuration["AZURE_OPENAI_API_KEY"] ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY configuration is required");
        _deploymentName = configuration["AZURE_OPENAI_DEPLOYMENT_NAME"] ?? "gpt-4";
        
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddAzureOpenAIChatCompletion(_deploymentName, endpoint, apiKey);
        _kernel = kernelBuilder.Build();

        _systemInstructions = CreateSystemInstructions();
    }

    public async Task<ConversationAnalysisResult> AnalyzeConversationAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting analysis for conversation {ConversationId}", conversation.Id);

            // Load conversation messages from database
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
            
            var conversationWithMessages = await db.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == conversation.Id, cancellationToken);

            if (conversationWithMessages == null)
            {
                throw new InvalidOperationException($"Conversation {conversation.Id} not found");
            }

            // Prepare conversation text for analysis
            var conversationText = PrepareConversationText(conversationWithMessages);
            
            if (string.IsNullOrWhiteSpace(conversationText))
            {
                _logger.LogWarning("Conversation {ConversationId} has no analyzable content", conversation.Id);
                return CreateEmptyResult(stopwatch.Elapsed);
            }

            // Create analysis prompt
            var analysisPrompt = CreateAnalysisPrompt(conversationText, conversationWithMessages);
            
            // Get chat completion service
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            
            // Create chat history with system instructions
            var chatHistory = new ChatHistory();
            foreach (var instruction in _systemInstructions)
            {
                chatHistory.Add(instruction);
            }
            chatHistory.AddUserMessage(analysisPrompt);

            // Execute analysis
            var result = await chatCompletionService.GetChatMessageContentAsync(
                chatHistory,
                new OpenAIPromptExecutionSettings { MaxTokens = 2000, Temperature = 0.3 },
                _kernel,
                cancellationToken);

            // Parse the JSON response
            var analysisResult = ParseAnalysisResponse(result.Content ?? string.Empty, stopwatch.Elapsed);
            analysisResult.ProcessingModel = _deploymentName;

            _logger.LogInformation("Completed analysis for conversation {ConversationId} in {Duration}ms", 
                conversation.Id, stopwatch.ElapsedMilliseconds);

            return analysisResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze conversation {ConversationId}", conversation.Id);
            
            return new ConversationAnalysisResult
            {
                SentimentScore = 0,
                SentimentLabel = "Unknown",
                ProcessingModel = _deploymentName,
                ProcessingDuration = stopwatch.Elapsed,
                Warnings = new List<string> { $"Analysis failed: {ex.Message}" }
            };
        }
    }

    public async Task<Dictionary<int, ConversationAnalysisResult>> AnalyzeConversationsAsync(
        IEnumerable<Conversation> conversations, 
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<int, ConversationAnalysisResult>();
        
        foreach (var conversation in conversations)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                var result = await AnalyzeConversationAsync(conversation, cancellationToken);
                results[conversation.Id] = result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to analyze conversation {ConversationId} in batch", conversation.Id);
                results[conversation.Id] = new ConversationAnalysisResult
                {
                    SentimentScore = 0,
                    SentimentLabel = "Error",
                    ProcessingModel = _deploymentName,
                    Warnings = new List<string> { $"Batch analysis failed: {ex.Message}" }
                };
            }
        }

        return results;
    }

    private ChatHistory CreateSystemInstructions()
    {
        var instructions = @"
You are an expert conversation analyst specializing in customer support interactions. 
Your task is to analyze conversation transcripts and provide structured insights.

Analyze the conversation and respond with a JSON object containing:
{
  ""sentimentScore"": decimal from -1.0 to 1.0 (negative to positive),
  ""sentimentLabel"": ""Positive"" | ""Neutral"" | ""Negative"",
  ""keyThemes"": [""theme1"", ""theme2""],
  ""resolutionSuccess"": true | false | null,
  ""customerSatisfactionIndicators"": [""indicator1"", ""indicator2""],
  ""recommendations"": [""recommendation1"", ""recommendation2""]
}

Guidelines:
- sentimentScore: Overall emotional tone (-1.0=very negative, 0=neutral, 1.0=very positive)
- sentimentLabel: Summary of overall sentiment
- keyThemes: Main topics, issues, or concerns discussed (max 5)
- resolutionSuccess: true if issue resolved, false if not, null if unclear
- customerSatisfactionIndicators: Signs of satisfaction/dissatisfaction
- recommendations: Actionable improvements for support quality (max 5)

Focus on customer experience, support quality, and actionable insights.
Respond only with valid JSON.";

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(instructions.Trim());
        return chatHistory;
    }

    private string PrepareConversationText(Conversation conversation)
    {
        if (conversation.Messages == null || !conversation.Messages.Any())
        {
            return string.Empty;
        }

        var messages = conversation.Messages
            .OrderBy(m => m.Timestamp)
            .Select(m => $"{m.Role}: {m.Content}")
            .ToList();

        return string.Join("\n", messages);
    }

    private string CreateAnalysisPrompt(string conversationText, Conversation conversation)
    {
        return $@"
Please analyze the following customer support conversation:

Conversation Details:
- Status: {conversation.Status}
- Priority: {conversation.Priority}
- Started: {conversation.StartTime:yyyy-MM-dd HH:mm}
- Duration: {conversation.Duration}
- Message Count: {conversation.MessageCount}

Conversation Transcript:
{conversationText}

Provide your analysis as JSON following the specified format.";
    }

    private ConversationAnalysisResult ParseAnalysisResponse(string jsonResponse, TimeSpan processingDuration)
    {
        try
        {
            // Clean up the response to extract JSON
            var jsonStart = jsonResponse.IndexOf('{');
            var jsonEnd = jsonResponse.LastIndexOf('}');
            
            if (jsonStart >= 0 && jsonEnd >= 0 && jsonEnd > jsonStart)
            {
                var cleanJson = jsonResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var analysisData = JsonSerializer.Deserialize<JsonElement>(cleanJson);
                
                return new ConversationAnalysisResult
                {
                    SentimentScore = analysisData.GetProperty("sentimentScore").GetDecimal(),
                    SentimentLabel = analysisData.GetProperty("sentimentLabel").GetString() ?? "Unknown",
                    KeyThemes = analysisData.GetProperty("keyThemes").EnumerateArray()
                        .Select(x => x.GetString() ?? "").Where(x => !string.IsNullOrEmpty(x)).ToList(),
                    ResolutionSuccess = analysisData.TryGetProperty("resolutionSuccess", out var resSuccess) && resSuccess.ValueKind != JsonValueKind.Null 
                        ? resSuccess.GetBoolean() : null,
                    CustomerSatisfactionIndicators = analysisData.GetProperty("customerSatisfactionIndicators").EnumerateArray()
                        .Select(x => x.GetString() ?? "").Where(x => !string.IsNullOrEmpty(x)).ToList(),
                    Recommendations = analysisData.GetProperty("recommendations").EnumerateArray()
                        .Select(x => x.GetString() ?? "").Where(x => !string.IsNullOrEmpty(x)).ToList(),
                    ProcessingDuration = processingDuration
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse analysis response: {Response}", jsonResponse);
            
            return new ConversationAnalysisResult
            {
                SentimentScore = 0,
                SentimentLabel = "Parse Error",
                ProcessingDuration = processingDuration,
                Warnings = new List<string> { $"Failed to parse AI response: {ex.Message}" }
            };
        }

        return CreateEmptyResult(processingDuration);
    }

    private ConversationAnalysisResult CreateEmptyResult(TimeSpan processingDuration)
    {
        return new ConversationAnalysisResult
        {
            SentimentScore = 0,
            SentimentLabel = "No Data",
            ProcessingDuration = processingDuration,
            Warnings = new List<string> { "No analyzable conversation content found" }
        };
    }
}