// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Common.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class ConversationParticipant : BaseAuditableEntity, IMustHaveTenant
{
    public int ConversationId { get; set; }
    public virtual Conversation Conversation { get; set; } = default!;
    
    public ParticipantType Type { get; set; }
    public string? ParticipantId { get; set; } // ApplicationUserId for agents/users, null for bot
    public string? ParticipantName { get; set; }
    public string? WhatsAppPhoneNumber { get; set; } // For customer participants
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string TenantId { get; set; } = default!;
    
    public TimeSpan? ParticipationDuration => LeftAt?.Subtract(JoinedAt);
}