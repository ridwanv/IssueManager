// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Entities;

namespace CleanArchitecture.Blazor.Application.Common.Interfaces;

/// <summary>
/// Service for analyzing conversations using AI to extract insights
/// </summary>
public interface IConversationAnalysisService
{
    /// <summary>
    /// Analyzes a conversation to extract sentiment, themes, and recommendations
    /// </summary>
    /// <param name="conversation">The conversation to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Conversation analysis results</returns>
    Task<ConversationAnalysisResult> AnalyzeConversationAsync(Conversation conversation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch analyzes multiple conversations
    /// </summary>
    /// <param name="conversations">Conversations to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary mapping conversation IDs to analysis results</returns>
    Task<Dictionary<int, ConversationAnalysisResult>> AnalyzeConversationsAsync(IEnumerable<Conversation> conversations, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of conversation analysis containing insights
/// </summary>
public class ConversationAnalysisResult
{
    /// <summary>
    /// Sentiment score from -1.0 (negative) to 1.0 (positive)
    /// </summary>
    public decimal SentimentScore { get; set; }

    /// <summary>
    /// Human-readable sentiment label (Positive, Neutral, Negative)
    /// </summary>
    public string SentimentLabel { get; set; } = default!;

    /// <summary>
    /// Key themes and topics extracted from the conversation
    /// </summary>
    public List<string> KeyThemes { get; set; } = new();

    /// <summary>
    /// Whether the issue was resolved successfully (null if unclear)
    /// </summary>
    public bool? ResolutionSuccess { get; set; }

    /// <summary>
    /// Indicators of customer satisfaction
    /// </summary>
    public List<string> CustomerSatisfactionIndicators { get; set; } = new();

    /// <summary>
    /// Improvement suggestions and recommendations
    /// </summary>
    public List<string> Recommendations { get; set; } = new();

    /// <summary>
    /// AI model used for analysis
    /// </summary>
    public string ProcessingModel { get; set; } = default!;

    /// <summary>
    /// Duration of the analysis process
    /// </summary>
    public TimeSpan ProcessingDuration { get; set; }

    /// <summary>
    /// Any errors or warnings during analysis
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}