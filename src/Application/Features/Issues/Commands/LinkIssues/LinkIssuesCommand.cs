// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces.Caching;
using CleanArchitecture.Blazor.Application.Features.Issues.Caching;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Application.Features.Issues.Commands.LinkIssues;

public class LinkIssuesCommand : ICacheInvalidatorRequest<Result<Guid>>
{
    /// <summary>
    /// ID of the parent/primary issue
    /// </summary>
    public Guid ParentIssueId { get; set; }
    
    /// <summary>
    /// ID of the child/linked issue
    /// </summary>
    public Guid ChildIssueId { get; set; }
    
    /// <summary>
    /// Type of relationship between the issues
    /// </summary>
    public IssueLinkType LinkType { get; set; }
    
    /// <summary>
    /// Optional confidence score from automatic detection (0.0-1.0)
    /// </summary>
    public decimal? ConfidenceScore { get; set; }
    
    /// <summary>
    /// Whether this link is being created automatically by the system
    /// </summary>
    public bool CreatedBySystem { get; set; } = false;
    
    /// <summary>
    /// Additional metadata about the link (optional)
    /// </summary>
    public string? Metadata { get; set; }
    
    /// <summary>
    /// Reason for creating the link (for audit purposes)
    /// </summary>
    public string? Reason { get; set; }

    public string[] CacheKeys => [IssueCacheKey.GetCacheKey($"{ParentIssueId}"), IssueCacheKey.GetCacheKey($"{ChildIssueId}")];
    public IEnumerable<string>? Tags => ["issues"];
}