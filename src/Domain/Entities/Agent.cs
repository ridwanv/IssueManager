// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Common.Entities;
using CleanArchitecture.Blazor.Domain.Identity;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class Agent : BaseAuditableEntity, IMustHaveTenant
{
    public string ApplicationUserId { get; set; } = default!;
    public virtual ApplicationUser ApplicationUser { get; set; } = default!;
    
    public AgentStatus Status { get; set; } = AgentStatus.Offline;
    public int MaxConcurrentConversations { get; set; } = 5;
    public int ActiveConversationCount { get; set; } = 0;
    public DateTime? LastActiveAt { get; set; }
    public string? Skills { get; set; } // JSON array of skills/departments
    public int Priority { get; set; } = 1; // Higher priority agents get assigned first
    public string? Notes { get; set; }
    public string TenantId { get; set; } = default!;

    // Note: Conversations are linked via CurrentAgentId to ApplicationUserId, not Agent.Id
    
    public bool IsAvailable => Status == AgentStatus.Available && ActiveConversationCount < MaxConcurrentConversations;
    public bool CanTakeConversations => IsAvailable && Status != AgentStatus.Break && Status != AgentStatus.Offline;
}