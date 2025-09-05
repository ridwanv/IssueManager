// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;

public record AgentDto
{
    public int Id { get; set; }
    public required string ApplicationUserId { get; set; }
    public string? UserName { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public AgentStatus Status { get; set; }
    public int MaxConcurrentConversations { get; set; }
    public int ActiveConversationCount { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public string? Skills { get; set; }
    public int Priority { get; set; }
    public string? Notes { get; set; }
    public DateTime Created { get; set; }
    public string TenantId { get; set; } = default!;
    
    // Computed properties
    public bool IsAvailable => Status == AgentStatus.Available && ActiveConversationCount < MaxConcurrentConversations;
    public bool CanTakeConversations => IsAvailable && Status != AgentStatus.Break && Status != AgentStatus.Offline;
    public string StatusText => Status.ToString();
    public double WorkloadPercentage => MaxConcurrentConversations > 0 
        ? (double)ActiveConversationCount / MaxConcurrentConversations * 100 
        : 0;
}