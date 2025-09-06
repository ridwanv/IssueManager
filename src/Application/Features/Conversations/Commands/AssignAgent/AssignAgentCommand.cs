// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Commands.AssignAgent;

public record AssignAgentCommand(
    string ConversationId,
    string AgentId
) : ICacheInvalidatorRequest<Result<bool>>
{
    public string CacheKey => $"conversations-{ConversationId}";
    public IEnumerable<string>? Tags => new[] { "conversations", "agents" };
    public CancellationToken CancellationToken { get; set; }
}

public class AssignAgentCommandHandler : IRequestHandler<AssignAgentCommand, Result<bool>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IApplicationHubWrapper _hubWrapper;
    
    public AssignAgentCommandHandler(
        IApplicationDbContextFactory dbContextFactory,
        IApplicationHubWrapper hubWrapper)
    {
        _dbContextFactory = dbContextFactory;
        _hubWrapper = hubWrapper;
    }
    
    public async Task<Result<bool>> Handle(AssignAgentCommand request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
        
        // Get conversation
        var conversation = await db.Conversations
            .FirstOrDefaultAsync(c => c.ConversationReference == request.ConversationId, cancellationToken);
            
        if (conversation == null)
        {
            return Result<bool>.Failure("Conversation not found");
        }
        
        // Get agent
        var agent = await db.Agents
            .Include(a => a.ApplicationUser)
            .FirstOrDefaultAsync(a => a.ApplicationUserId == request.AgentId, cancellationToken);
            
        if (agent == null)
        {
            return Result<bool>.Failure("Agent not found");
        }
        
        if (!agent.CanTakeConversations)
        {
            return Result<bool>.Failure("Agent is not available to take conversations");
        }
        
        // Assign conversation
        conversation.CurrentAgentId = request.AgentId;
        conversation.Mode = ConversationMode.Human;
        conversation.LastActivityAt = DateTime.UtcNow;
        
        // Update agent conversation count
        agent.ActiveConversationCount++;
        
        // Update handoff status
        var pendingHandoff = await db.ConversationHandoffs
            .Where(h => h.ConversationReference == request.ConversationId && h.Status == HandoffStatus.Initiated)
            .OrderByDescending(h => h.InitiatedAt)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (pendingHandoff != null)
        {
            pendingHandoff.ToAgentId = request.AgentId;
            pendingHandoff.Status = HandoffStatus.Accepted;
            pendingHandoff.AcceptedAt = DateTime.UtcNow;
        }
        
        await db.SaveChangesAsync(cancellationToken);
        
        // Notify via SignalR
        var agentName = agent.ApplicationUser?.DisplayName ?? agent.ApplicationUser?.UserName ?? "Agent";
        await _hubWrapper.BroadcastConversationAssigned(request.ConversationId, request.AgentId, agentName);
        
        return Result<bool>.Success(true);
    }
}