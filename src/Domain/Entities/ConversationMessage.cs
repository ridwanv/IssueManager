// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Common.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class ConversationMessage : BaseAuditableEntity, IMustHaveTenant
{
    public int ConversationId { get; set; } // Foreign key to Conversation table
    public string BotFrameworkConversationId { get; set; } = default!; // Bot Framework conversation ID for direct lookup
    public string Role { get; set; } = default!; // "user", "assistant", "system", "tool"
    public string Content { get; set; } = default!; // Message content
    public string? ToolCallId { get; set; } // For tool call responses
    public string? ToolCalls { get; set; } // Serialized tool calls data (JSON)
    public string? ImageType { get; set; } // Image MIME type if image message
    public string? ImageData { get; set; } // Base64 encoded image data
    public string? Attachments { get; set; } // Serialized attachment data (JSON)
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? UserId { get; set; } // Bot Framework user ID
    public string? UserName { get; set; } // Bot Framework user name
    public string? ChannelId { get; set; } // Bot Framework channel ID
    public bool IsEscalated { get; set; } = false; // Whether this message was sent after escalation
    public string TenantId { get; set; } = default!;

    // Navigation properties
    public virtual Conversation? Conversation { get; set; }
    public virtual ICollection<ConversationAttachment> AttachmentEntities { get; set; } = new List<ConversationAttachment>();
}
