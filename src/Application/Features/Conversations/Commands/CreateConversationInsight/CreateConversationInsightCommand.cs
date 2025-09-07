// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces.Caching;
using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Commands.CreateConversationInsight;

/// <summary>
/// Command to create conversation insights from AI analysis results
/// </summary>
public class CreateConversationInsightCommand : ICacheInvalidatorRequest<Result<int>>
{
    [Required]
    public int ConversationId { get; set; }

    [Required]
    [Range(-1.0, 1.0)]
    public decimal SentimentScore { get; set; }

    [Required]
    [MaxLength(50)]
    public string SentimentLabel { get; set; } = default!;

    public List<string> KeyThemes { get; set; } = new();

    public bool? ResolutionSuccess { get; set; }

    public List<string> CustomerSatisfactionIndicators { get; set; } = new();

    public List<string> Recommendations { get; set; } = new();

    [Required]
    [MaxLength(50)]
    public string ProcessingModel { get; set; } = default!;

    [Required]
    public DateTime ProcessedAt { get; set; }

    [Required]
    public TimeSpan ProcessingDuration { get; set; }

    public List<string> Warnings { get; set; } = new();

    public string[] CacheKeys => [
        $"Conversation_{ConversationId}",
        $"ConversationInsights_{ConversationId}",
        "ConversationAnalytics",
        "RecentInsights"
    ];

    public IEnumerable<string>? Tags => ["conversations", "insights", "analysis"];
}