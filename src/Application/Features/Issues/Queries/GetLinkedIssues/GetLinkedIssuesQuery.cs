// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces.Caching;
using CleanArchitecture.Blazor.Application.Features.Issues.Caching;
using CleanArchitecture.Blazor.Application.Features.Issues.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.Issues.Queries.GetLinkedIssues;

public class GetLinkedIssuesQuery : ICacheableRequest<Result<LinkedIssuesDto>>
{
    /// <summary>
    /// ID of the issue to get linked issues for
    /// </summary>
    public Guid IssueId { get; set; }
    
    /// <summary>
    /// Whether to include detailed information about linked issues
    /// </summary>
    public bool IncludeDetails { get; set; } = true;

    public string CacheKey => IssueCacheKey.GetCacheKey($"linked_{IssueId}_{IncludeDetails}");
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public IEnumerable<string>? Tags => ["issues"];
}