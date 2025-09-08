// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using AutoMapper;
using CleanArchitecture.Blazor.Domain.Entities;

namespace CleanArchitecture.Blazor.Application.Features.Agents.DTOs;

public class AgentNotificationPreferencesDto
{
    public int Id { get; set; }
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
    
    public string TenantId { get; set; } = default!;
    
    // Helper methods
    public bool ShouldNotifyForPriority(int priority) => priority switch
    {
        1 => NotifyOnStandardPriority,
        2 => NotifyOnHighPriority,
        3 => NotifyOnCriticalPriority,
        _ => NotifyOnStandardPriority
    };
    
    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<AgentNotificationPreferences, AgentNotificationPreferencesDto>();
            CreateMap<AgentNotificationPreferencesDto, AgentNotificationPreferences>()
                .ForMember(dest => dest.ApplicationUser, opt => opt.Ignore())
                .ForMember(dest => dest.Created, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.LastModified, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DomainEvents, opt => opt.Ignore());
        }
    }
}