// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetTransferEligibleAgents;

public record GetTransferEligibleAgentsQuery(
    string ConversationId,
    string? CurrentAgentId = null
) : ICacheableRequest<Result<List<AgentDto>>>
{
    public string CacheKey => $"transfer-eligible-agents-{ConversationId}";
    public IEnumerable<string>? Tags => new[] { "agents", "conversations", "transfer" };
    public TimeSpan? Expiry => TimeSpan.FromMinutes(1);
}

public class GetTransferEligibleAgentsQueryHandler : IRequestHandler<GetTransferEligibleAgentsQuery, Result<List<AgentDto>>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    
    public GetTransferEligibleAgentsQueryHandler(
        IApplicationDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }
    
    public async Task<Result<List<AgentDto>>> Handle(GetTransferEligibleAgentsQuery request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

        // Verify conversation exists and get its status
        var conversation = await GetConversationAsync(db, request.ConversationId, cancellationToken);
        if (conversation == null)
        {
            return Result<List<AgentDto>>.Failure("Conversation not found");
        }
        
        // Check if conversation is in a state that allows transfers
        if (!CanTransferConversation(conversation))
        {
            return Result<List<AgentDto>>.Failure("Conversation cannot be transferred. Only active conversations can be transferred.");
        }

        // Get agents eligible for transfer
        var agents = await db.Agents
            .Include(a => a.ApplicationUser)
            .Where(a => 
                a.Status != AgentStatus.Offline &&
                a.ApplicationUserId != request.CurrentAgentId) // Exclude current agent
            .ToListAsync(cancellationToken);

        // Project to DTO with transfer-specific ordering
        var agentDtos = agents
            .Select(a => new AgentDto
            {
                Id = a.Id,
                ApplicationUserId = a.ApplicationUserId,
                UserName = a.ApplicationUser.UserName,
                DisplayName = a.ApplicationUser.DisplayName,
                Email = a.ApplicationUser.Email,
                Status = a.Status,
                MaxConcurrentConversations = a.MaxConcurrentConversations,
                ActiveConversationCount = a.ActiveConversationCount,
                LastActiveAt = a.LastActiveAt,
                Skills = a.Skills,
                Priority = a.Priority,
                Notes = a.Notes,
                Created = a.Created ?? DateTime.UtcNow,
                TenantId = a.TenantId
            })
            // Order by: Available agents first, then by workload (lowest first), then by priority (highest first)
            .OrderByDescending(a => a.CanTakeConversations)
            .ThenBy(a => a.WorkloadPercentage)
            .ThenByDescending(a => a.Priority)
            .ToList();

        return Result<List<AgentDto>>.Success(agentDtos);
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
}