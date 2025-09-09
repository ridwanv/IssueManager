// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Commands.TransferConversation;

public class TransferConversationCommandHandler : IRequestHandler<TransferConversationCommand, Result<bool>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IApplicationHubWrapper _hubWrapper;
    private readonly ILogger<TransferConversationCommandHandler> _logger;
    
    public TransferConversationCommandHandler(
        IApplicationDbContextFactory dbContextFactory,
        IApplicationHubWrapper hubWrapper,
        ILogger<TransferConversationCommandHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _hubWrapper = hubWrapper;
        _logger = logger;
    }
    
    public async Task<Result<bool>> Handle(TransferConversationCommand request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
        
        // Get conversation - support both ConversationReference (string) and Id (Guid) lookups
        var conversation = await GetConversationAsync(db, request.ConversationId, cancellationToken);
        if (conversation == null)
        {
            _logger.LogWarning("Conversation not found: {ConversationId}", request.ConversationId);
            return Result<bool>.Failure("Conversation not found");
        }
        
        // Check if conversation can be transferred
        if (!CanTransferConversation(conversation))
        {
            _logger.LogWarning("Conversation cannot be transferred. Status: {Status}, Id: {ConversationId}", 
                conversation.Status, request.ConversationId);
            return Result<bool>.Failure("Conversation cannot be transferred. Only active conversations can be transferred.");
        }
        
        // Get target agent
        var targetAgent = await db.Agents
            .Include(a => a.ApplicationUser)
            .FirstOrDefaultAsync(a => a.ApplicationUserId == request.ToAgentId, cancellationToken);
            
        if (targetAgent == null)
        {
            _logger.LogWarning("Target agent not found: {AgentId}", request.ToAgentId);
            return Result<bool>.Failure("Target agent not found");
        }
        
        // Validate agent availability and capacity
        if (!request.ForceTransfer && !CanAgentTakeConversation(targetAgent))
        {
            var reason = !targetAgent.CanTakeConversations ? "Agent is not available" : 
                        targetAgent.ActiveConversationCount >= targetAgent.MaxConcurrentConversations ? "Agent is at maximum capacity" :
                        "Agent cannot take conversations";
            _logger.LogWarning("Agent cannot take conversation: {AgentId}, Reason: {Reason}", request.ToAgentId, reason);
            return Result<bool>.Failure(reason);
        }
        
        // Get current agent for audit logging
        var fromAgentId = conversation.CurrentAgentId;
        var fromAgent = fromAgentId != null ? await db.Agents
            .Include(a => a.ApplicationUser)
            .FirstOrDefaultAsync(a => a.ApplicationUserId == fromAgentId, cancellationToken) : null;
        
        // Update conversation counts
        if (fromAgent != null && fromAgent.ActiveConversationCount > 0)
        {
            fromAgent.ActiveConversationCount--;
        }
        
        targetAgent.ActiveConversationCount++;
        
        // Transfer conversation
        conversation.CurrentAgentId = request.ToAgentId;
        conversation.LastActivityAt = DateTime.UtcNow;
        
        // Create transfer audit record
        var transferRecord = new ConversationHandoff
        {
            ConversationId = conversation.Id,
            ConversationReference = conversation.ConversationReference,
            HandoffType = HandoffType.AgentToAgent,
            FromParticipantType = ParticipantType.Agent,
            ToParticipantType = ParticipantType.Agent,
            FromAgentId = fromAgentId,
            ToAgentId = request.ToAgentId,
            Status = HandoffStatus.Accepted,
            InitiatedAt = DateTime.UtcNow,
            AcceptedAt = DateTime.UtcNow,
            Reason = request.Reason ?? "Agent transfer",
            TenantId = conversation.TenantId
        };
        
        db.ConversationHandoffs.Add(transferRecord);
        
        await db.SaveChangesAsync(cancellationToken);
        
        // Log transfer action
        _logger.LogInformation("Conversation transferred: {ConversationId} from {FromAgent} to {ToAgent}",
            request.ConversationId, 
            fromAgent?.ApplicationUser?.DisplayName ?? "System",
            targetAgent.ApplicationUser?.DisplayName ?? request.ToAgentId);
        
        // Notify via SignalR
        var toAgentName = targetAgent.ApplicationUser?.DisplayName ?? targetAgent.ApplicationUser?.UserName ?? "Agent";
        var fromAgentName = fromAgent?.ApplicationUser?.DisplayName ?? fromAgent?.ApplicationUser?.UserName ?? "System";
        
        await _hubWrapper.BroadcastConversationTransferred(
            conversation.ConversationReference, 
            fromAgentId, 
            request.ToAgentId, 
            fromAgentName, 
            toAgentName);
        
        return Result<bool>.Success(true);
    }
    
    private async Task<Domain.Entities.Conversation?> GetConversationAsync(IApplicationDbContext db, string conversationId, CancellationToken cancellationToken)
    {
        // Try to parse as int first
        if (int.TryParse(conversationId, out var id))
        {
            return await db.Conversations
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }
        
        // Fallback to ConversationReference lookup
        return await db.Conversations
            .FirstOrDefaultAsync(c => c.ConversationReference == conversationId, cancellationToken);
    }
    
    private static bool CanTransferConversation(Domain.Entities.Conversation conversation)
    {
        return conversation.Status == ConversationStatus.Active;
    }
    
    private static bool CanAgentTakeConversation(Domain.Entities.Agent agent)
    {
        return agent.CanTakeConversations && agent.ActiveConversationCount < agent.MaxConcurrentConversations;
    }
}