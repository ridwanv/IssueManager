using CleanArchitecture.Blazor.Application.Common.Interfaces.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetPendingEscalations;

public class GetPendingEscalationsQuery : ICacheableRequest<Result<List<PendingEscalationDto>>>
{
    public string CacheKey => "PendingEscalations";
    public MemoryCacheEntryOptions? Options => new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
    };
    public IEnumerable<string>? Tags => ["escalations", "conversations"];
}

public class PendingEscalationDto
{
    public string ConversationId { get; set; } = string.Empty;
    public string ConversationReference { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string EscalationReason { get; set; } = string.Empty;
    public int Priority { get; set; } = 1;
    public DateTime EscalatedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
}
