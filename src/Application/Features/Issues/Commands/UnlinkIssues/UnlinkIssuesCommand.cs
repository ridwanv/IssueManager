// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces.Caching;
using CleanArchitecture.Blazor.Application.Features.Issues.Caching;

namespace CleanArchitecture.Blazor.Application.Features.Issues.Commands.UnlinkIssues;

public class UnlinkIssuesCommand : ICacheInvalidatorRequest<Result<bool>>
{
    /// <summary>
    /// ID of the issue link to remove
    /// </summary>
    public Guid IssueLinkId { get; set; }
    
    /// <summary>
    /// Reason for unlinking the issues (for audit purposes)
    /// </summary>
    public string? Reason { get; set; }

    public string[] CacheKeys => [IssueCacheKey.GetCacheKey("*")]; // Invalidate all issue caches
    public IEnumerable<string>? Tags => ["issues"];
}