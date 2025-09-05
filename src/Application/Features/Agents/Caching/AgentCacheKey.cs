// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Application.Features.Agents.Caching;

public static class AgentCacheKey
{
    public const string GetAllCacheKey = "all-agents";
    public const string GetByIdCacheKey = "agent-by-id-{0}";
    public const string GetByUserIdCacheKey = "agent-by-user-id-{0}";
    public const string GetCurrentAgentKey = "current-agent";
    public const string GetAvailableAgentsCacheKey = "available-agents";
    
    public static string GetByIdKey(int id) => string.Format(GetByIdCacheKey, id);
    public static string GetByUserIdKey(string userId) => string.Format(GetByUserIdCacheKey, userId);
    
    public static IEnumerable<string> Tags => new[]
    {
        "agents",
        "agent-management"
    };
}
