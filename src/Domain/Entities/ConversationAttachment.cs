// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Common.Entities;

namespace CleanArchitecture.Blazor.Domain.Entities;

public class ConversationAttachment : BaseAuditableEntity, IMustHaveTenant
{
    public int ConversationId { get; set; }
    public int? MessageId { get; set; } // Optional link to specific message
    public string BotFrameworkConversationId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public string? Url { get; set; }
    public string? FileData { get; set; } // Base64 encoded file data for small files
    public long? FileSize { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string TenantId { get; set; } = default!;

    // Navigation properties
    public virtual Conversation? Conversation { get; set; }
    public virtual ConversationMessage? Message { get; set; }
}
