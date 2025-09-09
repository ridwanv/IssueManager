// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Commands.TransferConversation;

public record TransferConversationCommand(
    string ConversationId,
    string ToAgentId,
    string? Reason = null,
    bool ForceTransfer = false
) : ICacheInvalidatorRequest<Result<bool>>
{
    public string CacheKey => $"conversations-{ConversationId}";
    public IEnumerable<string>? Tags => new[] { "conversations", "agents" };
    public CancellationToken CancellationToken { get; set; }
}