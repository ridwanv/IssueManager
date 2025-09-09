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
        try
        {
            Console.WriteLine("[AcceptEscalationHandler] Starting handle method");
            
            // Validate request
            if (request == null)
            {
                Console.WriteLine("[AcceptEscalationHandler] Request is null");
                return Result<Unit>.Failure("Invalid request: request is null.");
            }
            
            if (string.IsNullOrEmpty(request.ConversationId))
            {
                Console.WriteLine("[AcceptEscalationHandler] ConversationId is null or empty");
                return Result<Unit>.Failure("Invalid request: missing conversation ID.");
            }

            Console.WriteLine($"[AcceptEscalationHandler] Processing conversation: {request.ConversationId}");
            
            Console.WriteLine("[AcceptEscalationHandler] Checking dbContextFactory");
            if (_dbContextFactory == null)
            {
                Console.WriteLine("[AcceptEscalationHandler] _dbContextFactory is null");
                return Result<Unit>.Failure("Database context factory is not available.");
            }

            Console.WriteLine("[AcceptEscalationHandler] Creating database context");
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
            
            Console.WriteLine("[AcceptEscalationHandler] Checking userContextAccessor");
            if (_userContextAccessor == null)
            {
                Console.WriteLine("[AcceptEscalationHandler] _userContextAccessor is null");
                return Result<Unit>.Failure("User context accessor is not available.");
            }
            
            var currentUser = _userContextAccessor.Current;
            if (currentUser == null)
            {
                Console.WriteLine("[AcceptEscalationHandler] Current user is null");
                return Result<Unit>.Failure("User not authenticated: current user is null.");
            }
            
            if (string.IsNullOrEmpty(currentUser.UserId))
            {
                Console.WriteLine("[AcceptEscalationHandler] Current user ID is null or empty");
                return Result<Unit>.Failure("User not authenticated: user ID is missing.");
            }

            var currentUserId = currentUser.UserId;
            Console.WriteLine($"[AcceptEscalationHandler] Current user: {currentUserId}");

            Console.WriteLine("[AcceptEscalationHandler] Querying conversation from database");
            // Find the conversation and check if it's still available for acceptance
            var conversation = await db.Conversations
                .Include(c => c.Handoffs)
                .FirstOrDefaultAsync(c => c.ConversationReference == request.ConversationId, cancellationToken);

            if (conversation == null)
            {
                Console.WriteLine($"[AcceptEscalationHandler] Conversation not found: {request.ConversationId}");
                return Result<Unit>.Failure("Conversation not found.");
            }

            Console.WriteLine($"[AcceptEscalationHandler] Found conversation: {conversation.Id}, Mode: {conversation.Mode}, CurrentAgentId: {conversation.CurrentAgentId}");

            // Check if conversation is still in escalating state and not assigned
            if (conversation.Mode != ConversationMode.Escalating || 
                !string.IsNullOrEmpty(conversation.CurrentAgentId))
            {
                Console.WriteLine($"[AcceptEscalationHandler] Conversation already accepted - Mode: {conversation.Mode}, CurrentAgentId: {conversation.CurrentAgentId}");
                return Result<Unit>.Failure("Conversation has already been accepted by another agent.");
            }

            Console.WriteLine("[AcceptEscalationHandler] Updating conversation");
            // Assign conversation to current agent
            conversation.CurrentAgentId = currentUserId;
            conversation.Mode = ConversationMode.Human;
            conversation.Status = ConversationStatus.Active;
            
            Console.WriteLine("[AcceptEscalationHandler] Creating handoff record");
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
            
            Console.WriteLine("[AcceptEscalationHandler] Saving changes to database");
            await db.SaveChangesAsync(cancellationToken);

            Console.WriteLine("[AcceptEscalationHandler] Notifying via SignalR");
            // Notify other agents that this escalation has been accepted
            if (_hubWrapper != null)
            {
                await _hubWrapper.NotifyEscalationAccepted(conversation.ConversationReference, currentUserId);
                Console.WriteLine("[AcceptEscalationHandler] SignalR notification sent");
            }
            else
            {
                Console.WriteLine("[AcceptEscalationHandler] _hubWrapper is null, skipping SignalR notification");
            }

            Console.WriteLine("[AcceptEscalationHandler] Success!");
            return await Result<Unit>.SuccessAsync(Unit.Value);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AcceptEscalationHandler] Exception: {ex}");
            Console.WriteLine($"[AcceptEscalationHandler] StackTrace: {ex.StackTrace}");
            return Result<Unit>.Failure($"An error occurred while accepting escalation: {ex.Message}");
        }
    }
}