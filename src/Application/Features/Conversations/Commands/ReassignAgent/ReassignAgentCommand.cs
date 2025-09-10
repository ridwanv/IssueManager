// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Commands.ReassignAgent;

public record ReassignAgentCommand(
    string ConversationId,
    string NewAgentId,
    string ReasonForReassignment = ""
) : ICacheInvalidatorRequest<Result<bool>>
{
    public string CacheKey => $"conversations-{ConversationId}";
    public IEnumerable<string>? Tags => new[] { "conversations", "agents", "assignments" };
    public CancellationToken CancellationToken { get; set; }
}

public class ReassignAgentCommandHandler : IRequestHandler<ReassignAgentCommand, Result<bool>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IApplicationHubWrapper _hubWrapper;
    private readonly IUserContextAccessor _currentUser;
    
    public ReassignAgentCommandHandler(
        IApplicationDbContextFactory dbContextFactory,
        IApplicationHubWrapper hubWrapper,
        IUserContextAccessor currentUser)
    {
        _dbContextFactory = dbContextFactory;
        _hubWrapper = hubWrapper;
        _currentUser = currentUser;
    }
    
    public async Task<Result<bool>> Handle(ReassignAgentCommand request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
        
        // Support both string (ConversationReference) and int (Id) lookups
        var conversation = await GetConversationAsync(db, request.ConversationId, cancellationToken);
        
        if (conversation == null)
        {
            return Result<bool>.Failure("Conversation not found");
        }
        
        // Get current agent if assigned
        var currentAgent = conversation.CurrentAgentId != null 
            ? await db.Agents
                .Include(a => a.ApplicationUser)
                .FirstOrDefaultAsync(a => a.ApplicationUserId == conversation.CurrentAgentId, cancellationToken)
            : null;
        
        // Get new agent
        var newAgent = await db.Agents
            .Include(a => a.ApplicationUser)
            .FirstOrDefaultAsync(a => a.ApplicationUserId == request.NewAgentId, cancellationToken);
            
        if (newAgent == null)
        {
            return Result<bool>.Failure("New agent not found");
        }
        
        if (!newAgent.CanTakeConversations)
        {
            return Result<bool>.Failure("New agent is not available to take conversations");
        }
        
        // Check if agent has capacity
        if (newAgent.ActiveConversationCount >= newAgent.MaxConcurrentConversations)
        {
            return Result<bool>.Failure("New agent has reached maximum conversation capacity");
        }
        
        var previousAgentId = conversation.CurrentAgentId;
        var previousAgentName = currentAgent?.ApplicationUser?.DisplayName ?? 
                               currentAgent?.ApplicationUser?.UserName ?? 
                               "Previous Agent";
        
        // Update conversation assignment
        conversation.CurrentAgentId = request.NewAgentId;
        conversation.Mode = ConversationMode.Human;
        conversation.LastActivityAt = DateTime.UtcNow;
        
        // Update agent conversation counts
        if (currentAgent != null)
        {
            currentAgent.ActiveConversationCount = Math.Max(0, currentAgent.ActiveConversationCount - 1);
        }
        
        newAgent.ActiveConversationCount++;
        
        // Update handoff status
        var pendingHandoff = await db.ConversationHandoffs
            .Where(h => h.ConversationReference == conversation.ConversationReference && 
                       h.Status == HandoffStatus.Accepted)
            .OrderByDescending(h => h.AcceptedAt)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (pendingHandoff != null)
        {
            // Complete the previous handoff
            pendingHandoff.Status = HandoffStatus.Completed;
            pendingHandoff.CompletedAt = DateTime.UtcNow;
        }
        
        // Create new handoff record for reassignment
        var reassignmentHandoff = new Domain.Entities.ConversationHandoff
        {
            ConversationId = conversation.Id,
            ConversationReference = conversation.ConversationReference,
            HandoffType = HandoffType.AgentToAgent,
            FromParticipantType = ParticipantType.Agent,
            ToParticipantType = ParticipantType.Agent,
            FromAgentId = previousAgentId,
            ToAgentId = request.NewAgentId,
            Status = HandoffStatus.Accepted,
            InitiatedAt = DateTime.UtcNow,
            AcceptedAt = DateTime.UtcNow,
            Reason = !string.IsNullOrEmpty(request.ReasonForReassignment) 
                ? request.ReasonForReassignment 
                : "Conversation reassigned by supervisor",
            TenantId = conversation.TenantId
        };
        
        db.ConversationHandoffs.Add(reassignmentHandoff);
        
        await db.SaveChangesAsync(cancellationToken);
        
        // Notify via SignalR
        var newAgentName = newAgent.ApplicationUser?.DisplayName ?? 
                          newAgent.ApplicationUser?.UserName ?? 
                          "Agent";
        
        await _hubWrapper.BroadcastConversationReassigned(
            conversation.ConversationReference,
            previousAgentId ?? "",
            request.NewAgentId,
            newAgentName,
            request.ReasonForReassignment
        );
        
        return Result<bool>.Success(true);
    }
    
    private async Task<Domain.Entities.Conversation?> GetConversationAsync(
        IApplicationDbContext db, 
        string conversationId, 
        CancellationToken cancellationToken)
    {
        // First try as ConversationReference (string)
        var conversation = await db.Conversations
            .FirstOrDefaultAsync(c => c.ConversationReference == conversationId, cancellationToken);
            
        if (conversation != null)
            return conversation;
        
        // Try as Id (int) if string lookup failed
        if (conversation == null && int.TryParse(conversationId, out var intId))
        {
            conversation = await db.Conversations
                .FirstOrDefaultAsync(c => c.Id == intId, cancellationToken);
        }
        
        return conversation;
    }
}