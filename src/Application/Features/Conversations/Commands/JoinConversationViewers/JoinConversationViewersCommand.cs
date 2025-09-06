using CleanArchitecture.Blazor.Application.Common.Interfaces.Caching;
using CleanArchitecture.Blazor.Application.Common.Models;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Commands.JoinConversationViewers;

public class JoinConversationViewersCommand : ICacheInvalidatorRequest<Result<Unit>>
{
    public string ConversationId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;

    public JoinConversationViewersCommand(string conversationId, string userId)
    {
        ConversationId = conversationId;
        UserId = userId;
    }

    public string[] CacheKeys => new[]
    {
        $"GetConversationViewers_{ConversationId}",
        $"GetConversationById_{ConversationId}"
    };
    
    public IEnumerable<string>? Tags => new[] { $"Conversation_{ConversationId}" };
}