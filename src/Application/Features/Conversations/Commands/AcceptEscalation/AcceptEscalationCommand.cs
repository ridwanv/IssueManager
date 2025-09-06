using CleanArchitecture.Blazor.Application.Common.Interfaces.Caching;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Commands.AcceptEscalation;

public class AcceptEscalationCommand : ICacheInvalidatorRequest<Result<Unit>>
{
    public string ConversationId { get; set; }
    
    public string[] CacheKeys => [
        $"ConversationContext_{ConversationId}",
        $"Conversation_{ConversationId}",
        "AvailableEscalations",
        "AgentDashboard"
    ];
    
    public IEnumerable<string>? Tags => ["conversations", "escalations"];
}