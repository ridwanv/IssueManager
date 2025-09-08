using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetPendingEscalations;

public class GetPendingEscalationsQueryHandler : IRequestHandler<GetPendingEscalationsQuery, Result<List<PendingEscalationDto>>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;

    public GetPendingEscalationsQueryHandler(IApplicationDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Result<List<PendingEscalationDto>>> Handle(GetPendingEscalationsQuery request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

        var pendingEscalations = await db.Conversations
            .Where(c => c.Mode == ConversationMode.Escalating && 
                       string.IsNullOrEmpty(c.CurrentAgentId) && 
                       c.Status == ConversationStatus.Active)
            .Select(c => new PendingEscalationDto
            {
                ConversationId = c.Id.ToString(),
                ConversationReference = c.ConversationReference,
                CustomerName = c.UserName ?? "Unknown Customer",
                PhoneNumber = c.WhatsAppPhoneNumber ?? "No phone",
                EscalationReason = c.EscalationReason ?? "Escalation requested",
                Priority = c.Priority,
                EscalatedAt = c.EscalatedAt ?? c.Created!.Value,
                LastActivityAt = c.LastActivityAt
            })
            .OrderByDescending(e => e.Priority)
            .ThenByDescending(e => e.EscalatedAt)
            .Take(50) // Limit to prevent performance issues
            .ToListAsync(cancellationToken);

        return await Result<List<PendingEscalationDto>>.SuccessAsync(pendingEscalations);
    }
}
