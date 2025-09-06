// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Features.Agents.DTOs;
using CleanArchitecture.Blazor.Application.Features.Agents.Caching;

namespace CleanArchitecture.Blazor.Application.Features.Agents.Commands.UpdatePreferences;

public class UpdateAgentPreferencesCommand : ICacheInvalidatorRequest<Result<AgentNotificationPreferencesDto>>
{
    public string ApplicationUserId { get; set; } = default!;
    
    public bool EnableBrowserNotifications { get; set; } = true;
    public bool EnableAudioAlerts { get; set; } = true;
    public bool EnableEmailNotifications { get; set; } = false;
    
    public bool NotifyOnStandardPriority { get; set; } = true;
    public bool NotifyOnHighPriority { get; set; } = true;
    public bool NotifyOnCriticalPriority { get; set; } = true;
    
    public bool NotifyDuringBreak { get; set; } = false;
    public bool NotifyWhenOffline { get; set; } = false;
    
    public int AudioVolume { get; set; } = 50;
    public string? CustomSoundUrl { get; set; }
    
    public string[] CacheKeys => [AgentCacheKey.GetPreferencesKey(ApplicationUserId)];
    public IEnumerable<string> Tags => ["Agents"];
}