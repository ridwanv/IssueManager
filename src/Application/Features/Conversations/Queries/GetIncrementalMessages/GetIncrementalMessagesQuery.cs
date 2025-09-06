using CleanArchitecture.Blazor.Application.Common.Interfaces.Caching;
using CleanArchitecture.Blazor.Application.Common.Models;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetIncrementalMessages;

public class GetIncrementalMessagesQuery : ICacheableRequest<Result<List<ConversationMessageDto>>>
{
    public string ConversationId { get; set; } = string.Empty;
    public DateTime? LastMessageTimestamp { get; set; }

    public GetIncrementalMessagesQuery(string conversationId, DateTime? lastMessageTimestamp = null)
    {
        ConversationId = conversationId;
        LastMessageTimestamp = lastMessageTimestamp;
    }

    public string CacheKey => $"GetIncrementalMessages_{ConversationId}_{LastMessageTimestamp:yyyy-MM-dd-HH-mm-ss}";
    public MemoryCacheEntryOptions? Options => null;
    public IEnumerable<string>? Tags => new[] { $"Conversation_{ConversationId}" };
}