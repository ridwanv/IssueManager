// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Application.Features.Issues.DTOs;

public class LinkedIssuesDto
{
    public Guid IssueId { get; set; }
    public string IssueReferenceNumber { get; set; } = default!;
    public string IssueTitle { get; set; } = default!;
    public List<IssueLinkDto> ParentLinks { get; set; } = new();
    public List<IssueLinkDto> ChildLinks { get; set; } = new();
    public int TotalLinkedCount => ParentLinks.Count + ChildLinks.Count;
    
    /// <summary>
    /// Summary of issue impact (how many users are affected)
    /// </summary>
    public IssueImpactSummaryDto ImpactSummary { get; set; } = new();
}

public class IssueLinkDto
{
    public Guid Id { get; set; }
    public Guid ParentIssueId { get; set; }
    public Guid ChildIssueId { get; set; }
    public IssueLinkType LinkType { get; set; }
    public decimal? ConfidenceScore { get; set; }
    public bool CreatedBySystem { get; set; }
    public string? Metadata { get; set; }
    public DateTime? Created { get; set; }
    public string? CreatedBy { get; set; }
    
    // Related issue information
    public IssueDto RelatedIssue { get; set; } = default!;
    
    /// <summary>
    /// Human-readable description of the link relationship
    /// </summary>
    public string RelationshipDescription { get; set; } = default!;
}

public class IssueImpactSummaryDto
{
    /// <summary>
    /// Total number of affected users across all linked issues
    /// </summary>
    public int AffectedUserCount { get; set; }
    
    /// <summary>
    /// List of unique contact details from all linked issues
    /// </summary>
    public List<AffectedUserDto> AffectedUsers { get; set; } = new();
    
    /// <summary>
    /// Breakdown of issues by priority
    /// </summary>
    public Dictionary<IssuePriority, int> PriorityBreakdown { get; set; } = new();
    
    /// <summary>
    /// Breakdown of issues by status
    /// </summary>
    public Dictionary<IssueStatus, int> StatusBreakdown { get; set; } = new();
    
    /// <summary>
    /// Date range of when issues were reported
    /// </summary>
    public DateTime? EarliestReported { get; set; }
    public DateTime? LatestReported { get; set; }
}

public class AffectedUserDto
{
    public Guid IssueId { get; set; }
    public string IssueReference { get; set; } = default!;
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string IssueDescription { get; set; } = default!;
    public DateTime ReportedAt { get; set; }
    public IssuePriority Priority { get; set; }
    public IssueStatus Status { get; set; }
}