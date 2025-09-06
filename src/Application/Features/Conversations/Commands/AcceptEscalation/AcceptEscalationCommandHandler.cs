using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Commands.AcceptEscalation;

public class AcceptEscalationCommandHandler : IRequestHandler<AcceptEscalationCommand, Result<Unit>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IUserContextAccessor _userContextAccessor;
    private readonly IApplicationHubWrapper _hubWrapper;

    public AcceptEscalationCommandHandler(
        IApplicationDbContextFactory dbContextFactory,
        IUserContextAccessor userContextAccessor,
        IApplicationHubWrapper hubWrapper)
    {
        _dbContextFactory = dbContextFactory;
        _userContextAccessor = userContextAccessor;
        _hubWrapper = hubWrapper;
    }

    public async Task<Result<Unit>> Handle(AcceptEscalationCommand request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
        
        var currentUserId = _userContextAccessor.Current?.UserId;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Result<Unit>.Failure("User not authenticated.");
        }

        // Find the conversation and check if it's still available for acceptance
        var conversation = await db.Conversations
            .Include(c => c.Handoffs)
            .FirstOrDefaultAsync(c => c.ConversationReference == request.ConversationId, cancellationToken);

        if (conversation == null)
        {
            return Result<Unit>.Failure("Conversation not found.");
        }

        // Check if conversation is still in escalating state and not assigned
        if (conversation.Mode != ConversationMode.Escalating || 
            !string.IsNullOrEmpty(conversation.CurrentAgentId))
        {
            return Result<Unit>.Failure("Conversation has already been accepted by another agent.");
        }

        // Assign conversation to current agent
        conversation.CurrentAgentId = currentUserId;
        conversation.Mode = ConversationMode.Human;
        conversation.Status = ConversationStatus.Active;
        
        // Create handoff record
        var handoff = new CleanArchitecture.Blazor.Domain.Entities.ConversationHandoff
        {
            ConversationId = conversation.Id,
            ConversationReference = conversation.ConversationReference,
            HandoffType = HandoffType.BotToHuman,
            Status = HandoffStatus.Completed,
            FromAgentId = null, // From bot
            ToAgentId = currentUserId,
            FromParticipantType = ParticipantType.Bot,
            ToParticipantType = ParticipantType.Agent,
            Reason = "Agent accepted escalation",
            InitiatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            TenantId = conversation.TenantId
        };

        db.ConversationHandoffs.Add(handoff);
        
        await db.SaveChangesAsync(cancellationToken);

        // Notify other agents that this escalation has been accepted
        await _hubWrapper.NotifyEscalationAccepted(conversation.ConversationReference, currentUserId);

        return await Result<Unit>.SuccessAsync(Unit.Value);
    }
}