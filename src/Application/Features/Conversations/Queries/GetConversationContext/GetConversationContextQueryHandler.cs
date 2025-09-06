using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetConversationContext;

public class GetConversationContextQueryHandler : IRequestHandler<GetConversationContextQuery, Result<EscalationPopupDto>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;

    public GetConversationContextQueryHandler(IApplicationDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Result<EscalationPopupDto>> Handle(GetConversationContextQuery request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
        
        var conversation = await db.Conversations
            .Include(c => c.Messages)
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.ConversationReference == request.ConversationId, cancellationToken);

        if (conversation == null)
        {
            return Result<EscalationPopupDto>.Failure("Conversation not found.");
        }

        var customerParticipant = conversation.Participants
            .FirstOrDefault(p => p.Type == Domain.Enums.ParticipantType.Customer);

        var lastMessage = conversation.Messages
            .OrderByDescending(m => m.Timestamp)
            .FirstOrDefault();

        var conversationDuration = conversation.EscalatedAt.HasValue
            ? conversation.EscalatedAt.Value - (conversation.Created ?? conversation.StartTime)
            : DateTime.UtcNow - (conversation.Created ?? conversation.StartTime);

        var dto = new EscalationPopupDto
        {
            ConversationReference = conversation.ConversationReference,
            CustomerName = customerParticipant?.ParticipantName ?? "Unknown Customer",
            PhoneNumber = customerParticipant?.WhatsAppPhoneNumber ?? "Unknown",
            EscalationReason = conversation.EscalationReason ?? "No reason provided",
            Priority = conversation.Priority,
            EscalatedAt = conversation.EscalatedAt ?? DateTime.UtcNow,
            LastMessage = lastMessage?.Content,
            MessageCount = conversation.Messages.Count,
            ConversationDuration = conversationDuration,
            ConversationSummary = conversation.ConversationSummary
        };

        return Result<EscalationPopupDto>.Success(dto);
    }
}