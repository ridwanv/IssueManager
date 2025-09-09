using CleanArchitecture.Blazor.Application.Common.Interfaces.Caching;
using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Commands.AcceptEscalation;

public class AcceptEscalationCommand : ICacheInvalidatorRequest<Result<Unit>>
{
    [Required]
    public string ConversationId { get; set; } = string.Empty;
    
    public string[] CacheKeys => [
        $"ConversationContext_{ConversationId}",
        $"Conversation_{ConversationId}",
        "AvailableEscalations",
        "AgentDashboard"
    ];
    
    public IEnumerable<string>? Tags => ["conversations", "escalations"];
}