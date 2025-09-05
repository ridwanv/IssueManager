// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Common.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class Conversation : BaseAuditableEntity, IMustHaveTenant
{
    public string ConversationId { get; set; } = default!; // Bot Framework conversation ID
    public string? UserId { get; set; } // Bot Framework user ID
    public string? UserName { get; set; } // Bot Framework user name
    public string? WhatsAppPhoneNumber { get; set; }
    public ConversationStatus Status { get; set; } = ConversationStatus.Active;
    public ConversationMode Mode { get; set; } = ConversationMode.Bot;
    public int Priority { get; set; } = 1; // 1=Standard, 2=High, 3=Critical
    public string? CurrentAgentId { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EscalatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? EscalationReason { get; set; }
    public string? ConversationSummary { get; set; }
    public int MessageCount { get; set; } = 0;
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    public string? ThreadId { get; set; } // For OpenAI Assistant API integration
    public int MaxTurns { get; set; } = 10; // Maximum turns to keep in memory
    public string TenantId { get; set; } = default!;

    // Navigation properties
    public virtual ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
    public virtual ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
    public virtual ICollection<ConversationHandoff> Handoffs { get; set; } = new List<ConversationHandoff>();

    public bool IsEscalated => Mode == ConversationMode.Human || Mode == ConversationMode.Escalating;
    public bool IsActive => Status == ConversationStatus.Active;
    public TimeSpan Duration => (CompletedAt ?? DateTime.UtcNow).Subtract(Created ?? StartTime);
    
    // Helper methods based on ConversationData
    public string GetLastUserMessage()
    {
        return Messages?.Where(m => m.Role == "user")
                       .OrderByDescending(m => m.Timestamp)
                       .FirstOrDefault()?.Content ?? "No messages";
    }

    public int GetTurnCount()
    {
        return Messages?.Count(m => m.Role == "user" || m.Role == "assistant") ?? 0;
    }

    public bool HasAttachments()
    {
        return Messages?.Any(m => !string.IsNullOrEmpty(m.Attachments) || !string.IsNullOrEmpty(m.ImageData)) ?? false;
    }
}