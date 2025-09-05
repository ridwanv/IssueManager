// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetAvailableAgents;

public record GetAvailableAgentsQuery() : ICacheableRequest<Result<List<AgentDto>>>
{
    public string CacheKey => "available-agents";
    public IEnumerable<string>? Tags => new[] { "agents" };
    public TimeSpan? Expiry => TimeSpan.FromMinutes(2);
}

public class GetAvailableAgentsQueryHandler : IRequestHandler<GetAvailableAgentsQuery, Result<List<AgentDto>>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    
    public GetAvailableAgentsQueryHandler(IApplicationDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }
    
    public async Task<Result<List<AgentDto>>> Handle(GetAvailableAgentsQuery request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

        // Fetch agents from DB (no ordering by IsAvailable)
        var agents = await db.Agents
            .Include(a => a.ApplicationUser)
            .Where(a => a.Status != AgentStatus.Offline)
            .ToListAsync(cancellationToken);

        // Project to DTO and order in memory by IsAvailable, ActiveConversationCount, Priority
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
            .OrderByDescending(a => a.IsAvailable)
            .ThenBy(a => a.ActiveConversationCount)
            .ThenByDescending(a => a.Priority)
            .ToList();

        return Result<List<AgentDto>>.Success(agentDtos);
    }
}