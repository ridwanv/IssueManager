// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Application.Common.Interfaces;

public interface IIssueSimilarityService
{
    /// <summary>
    /// Finds similar issues using OpenAI semantic analysis within a specified timeframe
    /// </summary>
    /// <param name="title">Title of the new issue</param>
    /// <param name="description">Description of the new issue</param>
    /// <param name="category">Category of the new issue</param>
    /// <param name="priority">Priority of the new issue</param>
    /// <param name="product">Product/system affected</param>
    /// <param name="tenantId">Tenant context</param>
    /// <param name="timeframeHours">Hours to look back for similar issues (default: 168 = 7 days)</param>
    /// <param name="confidenceThreshold">Minimum confidence threshold for similarity (0.0-1.0, default: 0.8)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of similar issues with their confidence scores</returns>
    Task<IEnumerable<SimilarIssueResult>> FindSimilarIssuesAsync(
        string title,
        string description,
        IssueCategory category,
        IssuePriority priority,
        string? product,
        string tenantId,
        int timeframeHours = 168,
        decimal confidenceThreshold = 0.8m,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compares two specific issues for similarity using OpenAI
    /// </summary>
    /// <param name="issue1">First issue to compare</param>
    /// <param name="issue2">Second issue to compare</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Similarity result with confidence score and analysis</returns>
    Task<SimilarityComparison> CompareIssuesAsync(
        Issue issue1,
        Issue issue2,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes the potential relationship type between two similar issues
    /// </summary>
    /// <param name="existingIssue">The existing issue</param>
    /// <param name="newIssue">The new issue being reported</param>
    /// <param name="confidenceScore">Similarity confidence score</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recommended relationship type and reasoning</returns>
    Task<IssueLinkRecommendation> AnalyzeRelationshipAsync(
        Issue existingIssue,
        Issue newIssue,
        decimal confidenceScore,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of similarity detection for a single issue
/// </summary>
public class SimilarIssueResult
{
    public Issue Issue { get; set; } = default!;
    public decimal ConfidenceScore { get; set; }
    public string Reasoning { get; set; } = default!;
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Result of comparing two specific issues
/// </summary>
public class SimilarityComparison
{
    public decimal ConfidenceScore { get; set; }
    public bool IsSimilar { get; set; }
    public string Analysis { get; set; } = default!;
    public Dictionary<string, string> Details { get; set; } = new();
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Recommendation for issue linking relationship
/// </summary>
public class IssueLinkRecommendation
{
    public IssueLinkType RecommendedLinkType { get; set; }
    public decimal Confidence { get; set; }
    public string Reasoning { get; set; } = default!;
    public bool ShouldAutoLink { get; set; }
    public string SuggestedResponse { get; set; } = default!;
}