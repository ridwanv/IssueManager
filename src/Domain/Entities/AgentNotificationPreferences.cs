// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Common.Entities;
using CleanArchitecture.Blazor.Domain.Identity;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class AgentNotificationPreferences : BaseAuditableEntity, IMustHaveTenant
{
    public string ApplicationUserId { get; set; } = default!;
    public virtual ApplicationUser ApplicationUser { get; set; } = default!;
    
    public bool EnableBrowserNotifications { get; set; } = true;
    public bool EnableAudioAlerts { get; set; } = true;
    public bool EnableEmailNotifications { get; set; } = false;
    
    // Priority-based preferences
    public bool NotifyOnStandardPriority { get; set; } = true;
    public bool NotifyOnHighPriority { get; set; } = true;
    public bool NotifyOnCriticalPriority { get; set; } = true;
    
    // Notification timing
    public bool NotifyDuringBreak { get; set; } = false;
    public bool NotifyWhenOffline { get; set; } = false;
    
    // Audio preferences
    public int AudioVolume { get; set; } = 50; // 0-100
    public string? CustomSoundUrl { get; set; }
    
    public string TenantId { get; set; } = default!;
    
    // Helper methods
    public bool ShouldNotifyForPriority(int priority) => priority switch
    {
        1 => NotifyOnStandardPriority,
        2 => NotifyOnHighPriority,
        3 => NotifyOnCriticalPriority,
        _ => NotifyOnStandardPriority
    };
}