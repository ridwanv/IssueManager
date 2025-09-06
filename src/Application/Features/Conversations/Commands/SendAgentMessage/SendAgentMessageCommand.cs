using CleanArchitecture.Blazor.Application.Common.Interfaces.Caching;
using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Commands.SendAgentMessage;

public class SendAgentMessageCommand : ICacheInvalidatorRequest<Result<Unit>>
{
    [Required]
    public string ConversationId { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public required string Content { get; set; }
    
    public string[] CacheKeys => [
        $"Conversation_{ConversationId}",
        $"ConversationContext_{ConversationId}",
        "AgentDashboard",
        "RecentConversations"
    ];
    
    public IEnumerable<string>? Tags => ["conversations", "messages"];
}