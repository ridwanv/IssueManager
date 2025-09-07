// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetConversationInsights;

/// <summary>
/// Query to retrieve conversation insights by conversation ID
/// </summary>
public record GetConversationInsightsByIdQuery(int ConversationId) : ICacheableRequest<Result<ConversationInsightDto?>>
{
    public string CacheKey => $"conversation-insights-{ConversationId}";
    public IEnumerable<string>? Tags => new[] { "conversations", "insights" };
    public TimeSpan? Expiry => TimeSpan.FromHours(1); // Cache insights for 1 hour
}

/// <summary>
/// Query to retrieve all conversation insights with optional filtering
/// </summary>
public record GetAllConversationInsightsQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? SentimentFilter = null,
    bool? ResolutionSuccessFilter = null,
    int PageNumber = 1,
    int PageSize = 20
) : ICacheableRequest<Result<List<ConversationInsightDto>>>
{
    public string CacheKey => $"all-conversation-insights-{FromDate?.ToString("yyyyMMdd")}-{ToDate?.ToString("yyyyMMdd")}-{SentimentFilter}-{ResolutionSuccessFilter}-{PageNumber}-{PageSize}";
    public IEnumerable<string>? Tags => new[] { "conversations", "insights", "analytics" };
    public TimeSpan? Expiry => TimeSpan.FromMinutes(15); // Cache analytics for 15 minutes
}

/// <summary>
/// Query to retrieve conversation insights analytics/summary
/// </summary>
public record GetConversationInsightsSummaryQuery(
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : ICacheableRequest<Result<ConversationInsightsSummaryDto>>
{
    public string CacheKey => $"conversation-insights-summary-{FromDate?.ToString("yyyyMMdd")}-{ToDate?.ToString("yyyyMMdd")}";
    public IEnumerable<string>? Tags => new[] { "conversations", "insights", "analytics", "summary" };
    public TimeSpan? Expiry => TimeSpan.FromMinutes(30); // Cache summary for 30 minutes
}

/// <summary>
/// Summary DTO for conversation insights analytics
/// </summary>
public record ConversationInsightsSummaryDto
{
    public int TotalInsights { get; set; }
    public decimal AverageSentimentScore { get; set; }
    public Dictionary<string, int> SentimentDistribution { get; set; } = new();
    public int SuccessfulResolutions { get; set; }
    public int UnsuccessfulResolutions { get; set; }
    public decimal ResolutionSuccessRate { get; set; }
    public List<string> TopKeyThemes { get; set; } = new();
    public List<string> TopRecommendations { get; set; } = new();
    public TimeSpan AverageProcessingDuration { get; set; }
    public Dictionary<string, int> ProcessingModelDistribution { get; set; } = new();
}