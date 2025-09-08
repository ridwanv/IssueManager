// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Commands.CompleteConversation;

public record CompleteConversationCommand(
    string ConversationId,
    ResolutionCategory Category,
    string ResolutionNotes,
    bool NotifyCustomer = true
) : ICacheInvalidatorRequest<Result<bool>>
{
    public string CacheKey => $"conversations-{ConversationId}";
    public IEnumerable<string>? Tags => new[] { "conversations", "agents" };
    public CancellationToken CancellationToken { get; set; }
}

public class CompleteConversationCommandHandler : IRequestHandler<CompleteConversationCommand, Result<bool>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IApplicationHubWrapper _hubWrapper;
    private readonly IUserContextAccessor _userContextAccessor;
    
    public CompleteConversationCommandHandler(
        IApplicationDbContextFactory dbContextFactory,
        IApplicationHubWrapper hubWrapper,
        IUserContextAccessor userContextAccessor)
    {
        _dbContextFactory = dbContextFactory;
        _hubWrapper = hubWrapper;
        _userContextAccessor = userContextAccessor;
    }
    
    public async Task<Result<bool>> Handle(CompleteConversationCommand request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
        
        var userContext = _userContextAccessor.Current;
        if (userContext == null || string.IsNullOrEmpty(userContext.UserId))
        {
            return Result<bool>.Failure("User not authenticated");
        }
        
        var currentUserId = userContext.UserId;
        
        // Get conversation
        var conversation = await db.Conversations
            .FirstOrDefaultAsync(c => c.ConversationReference == request.ConversationId, cancellationToken);
            
        if (conversation == null)
        {
            return Result<bool>.Failure("Conversation not found");
        }
        
        var agentId = conversation.CurrentAgentId;
        
        // Update conversation with resolution details
        conversation.Status = ConversationStatus.Completed;
        conversation.Mode = ConversationMode.Bot; // Back to bot for future messages
        conversation.CompletedAt = DateTime.UtcNow;
        conversation.LastActivityAt = DateTime.UtcNow;
        conversation.ResolutionCategory = request.Category;
        conversation.ResolutionNotes = request.ResolutionNotes;
        conversation.ResolvedByAgentId = currentUserId;
        
        // Clear agent assignment
        conversation.CurrentAgentId = null;
        
        // Update agent conversation count
        if (!string.IsNullOrEmpty(agentId))
        {
            var agent = await db.Agents
                .FirstOrDefaultAsync(a => a.ApplicationUserId == agentId, cancellationToken);
                
            if (agent != null && agent.ActiveConversationCount > 0)
            {
                agent.ActiveConversationCount--;
            }
        }
        
        // Complete handoff
        var activeHandoff = await db.ConversationHandoffs
            .Where(h => h.ConversationReference == request.ConversationId && h.Status == HandoffStatus.Accepted)
            .OrderByDescending(h => h.AcceptedAt)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (activeHandoff != null)
        {
            activeHandoff.Status = HandoffStatus.Completed;
            activeHandoff.CompletedAt = DateTime.UtcNow;
        }

        // TODO: Add audit trail entry for completion if needed (EventLog is Issue-specific)
        
        await db.SaveChangesAsync(cancellationToken);
        
        // Notify via SignalR
        await _hubWrapper.BroadcastConversationCompleted(request.ConversationId, agentId ?? "");
        
        return Result<bool>.Success(true);
    }
}