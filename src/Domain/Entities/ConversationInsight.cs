// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Common.Entities;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class ConversationInsight : BaseAuditableEntity, IMustHaveTenant
{
    public int ConversationId { get; set; } // Foreign key to parent conversation
    public decimal SentimentScore { get; set; } // Overall conversation sentiment (-1.0 to 1.0)
    public string SentimentLabel { get; set; } = default!; // Human readable sentiment (Positive, Neutral, Negative)
    public string KeyThemes { get; set; } = default!; // JSON array of extracted themes and topics
    public bool? ResolutionSuccess { get; set; } // Whether issue was resolved successfully
    public string CustomerSatisfactionIndicators { get; set; } = default!; // JSON array of satisfaction signals
    public string Recommendations { get; set; } = default!; // JSON array of improvement suggestions
    public string ProcessingModel { get; set; } = default!; // GPT model used for analysis (e.g., "gpt-4")
    public DateTime ProcessedAt { get; set; } // When analysis was completed
    public TimeSpan ProcessingDuration { get; set; } // How long analysis took
    public string TenantId { get; set; } = default!; // Multi-tenant isolation

    // Navigation property
    public virtual Conversation Conversation { get; set; } = default!;
}