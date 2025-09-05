// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Common.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class ConversationHandoff : BaseAuditableEntity, IMustHaveTenant
{
    public int ConversationId { get; set; }
    public virtual Conversation Conversation { get; set; } = default!;
    
    public HandoffType HandoffType { get; set; }
    public ParticipantType FromParticipantType { get; set; }
    public ParticipantType ToParticipantType { get; set; }
    
    public string? FromAgentId { get; set; } // If handing off from agent
    public string? ToAgentId { get; set; } // If handing off to agent
    
    public string Reason { get; set; } = default!;
    public string? ConversationTranscript { get; set; } // JSON of conversation history
    public string? ContextData { get; set; } // Additional context for the handoff
    
    public HandoffStatus Status { get; set; } = HandoffStatus.Initiated;
    public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public string TenantId { get; set; } = default!;
    
    public TimeSpan? HandoffDuration => CompletedAt?.Subtract(InitiatedAt);
    public bool IsCompleted => Status == HandoffStatus.Completed;
}