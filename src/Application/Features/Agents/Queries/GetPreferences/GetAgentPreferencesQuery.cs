// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Features.Agents.DTOs;
using CleanArchitecture.Blazor.Application.Features.Agents.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace CleanArchitecture.Blazor.Application.Features.Agents.Queries.GetPreferences;

public class GetAgentPreferencesQuery : ICacheableRequest<Result<AgentNotificationPreferencesDto?>>
{
    public string ApplicationUserId { get; set; } = default!;
    
    public string CacheKey => AgentCacheKey.GetPreferencesKey(ApplicationUserId);
    
    public MemoryCacheEntryOptions? Options => new MemoryCacheEntryOptions
    {
        SlidingExpiration = TimeSpan.FromMinutes(5)
    };
    
    public IEnumerable<string> Tags => ["Agents"];
}