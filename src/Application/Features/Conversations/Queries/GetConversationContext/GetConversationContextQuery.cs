using CleanArchitecture.Blazor.Application.Common.Interfaces.Caching;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetConversationContext;

public class GetConversationContextQuery : ICacheableRequest<Result<EscalationPopupDto>>
{
    public string ConversationId { get; set; }
    
    public string CacheKey => $"ConversationContext_{ConversationId}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(5);

    public IEnumerable<string>? Tags => new[] { "conversations", "context" };
}