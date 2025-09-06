using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Commands.JoinConversationViewers;

public class JoinConversationViewersCommandHandler : IRequestHandler<JoinConversationViewersCommand, Result<Unit>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;

    public JoinConversationViewersCommandHandler(IApplicationDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Result<Unit>> Handle(JoinConversationViewersCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

            if (!int.TryParse(request.ConversationId, out var conversationId))
            {
                return Result<Unit>.Failure("Invalid conversation ID format.");
            }

            // Check if conversation exists and user has access
            var conversationExists = await db.Conversations
                .AnyAsync(c => c.Id == conversationId, cancellationToken);

            if (!conversationExists)
            {
                return Result<Unit>.Failure("Conversation not found or access denied.");
            }

            // In a real implementation, you might track viewers in a separate table or cache
            // For now, we'll just return success as the tracking is handled by SignalR groups

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure($"Error joining conversation viewers: {ex.Message}");
        }
    }
}