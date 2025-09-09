// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetMyPastConversations;

public record GetMyPastConversationsQuery(
    string AgentId,
    string? SearchTerm = null,
    ConversationStatus? Status = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = "CompletedAt",
    bool SortDescending = true
) : IRequest<Result<PaginatedData<ConversationDto>>>;

public class GetMyPastConversationsQueryHandler : IRequestHandler<GetMyPastConversationsQuery, Result<PaginatedData<ConversationDto>>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMyPastConversationsQueryHandler> _logger;

    public GetMyPastConversationsQueryHandler(
        IApplicationDbContextFactory dbContextFactory,
        IMapper mapper,
        ILogger<GetMyPastConversationsQueryHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PaginatedData<ConversationDto>>> Handle(GetMyPastConversationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

            var query = db.Conversations.AsQueryable();

            // Filter for past conversations where the agent was involved
            var pastStatuses = new[] { ConversationStatus.Completed, ConversationStatus.Abandoned, ConversationStatus.Archived };
            
            if (request.Status.HasValue)
            {
                // Filter by specific past status if provided
                query = query.Where(c => c.Status == request.Status.Value && 
                                       (c.CurrentAgentId == request.AgentId || c.ResolvedByAgentId == request.AgentId));
            }
            else
            {
                // Filter by all past statuses if no specific status provided
                query = query.Where(c => pastStatuses.Contains(c.Status) && 
                                       (c.CurrentAgentId == request.AgentId || c.ResolvedByAgentId == request.AgentId));
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(c => c.CompletedAt >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(c => c.CompletedAt <= request.EndDate.Value);
            }

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(c => 
                    c.UserName!.Contains(request.SearchTerm) ||
                    c.ConversationSummary!.Contains(request.SearchTerm) ||
                    c.EscalationReason!.Contains(request.SearchTerm) ||
                    c.WhatsAppPhoneNumber!.Contains(request.SearchTerm) ||
                    c.ConversationReference.Contains(request.SearchTerm) ||
                    c.ResolutionNotes!.Contains(request.SearchTerm));
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "created" => request.SortDescending 
                    ? query.OrderByDescending(c => c.Created)
                    : query.OrderBy(c => c.Created),
                "status" => request.SortDescending 
                    ? query.OrderByDescending(c => c.Status)
                    : query.OrderBy(c => c.Status),
                "username" => request.SortDescending 
                    ? query.OrderByDescending(c => c.UserName)
                    : query.OrderBy(c => c.UserName),
                "messagecount" => request.SortDescending 
                    ? query.OrderByDescending(c => c.MessageCount)
                    : query.OrderBy(c => c.MessageCount),
                "duration" => request.SortDescending 
                    ? query.OrderByDescending(c => c.Duration)
                    : query.OrderBy(c => c.Duration),
                "resolutioncategory" => request.SortDescending 
                    ? query.OrderByDescending(c => c.ResolutionCategory)
                    : query.OrderBy(c => c.ResolutionCategory),
                "completedat" => request.SortDescending 
                    ? query.OrderByDescending(c => c.CompletedAt)
                    : query.OrderBy(c => c.CompletedAt),
                _ => request.SortDescending 
                    ? query.OrderByDescending(c => c.CompletedAt)
                    : query.OrderBy(c => c.CompletedAt)
            };

            // Get total count for pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var conversations = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var conversationDtos = _mapper.Map<List<ConversationDto>>(conversations);

            // Get agent names for both current agents and resolved by agents
            var currentAgentIds = conversationDtos
                .Where(c => !string.IsNullOrEmpty(c.CurrentAgentId))
                .Select(c => c.CurrentAgentId!)
                .Distinct();

            var resolvedByAgentIds = conversationDtos
                .Where(c => !string.IsNullOrEmpty(c.ResolvedByAgentId))
                .Select(c => c.ResolvedByAgentId!)
                .Distinct();

            var allAgentIds = currentAgentIds.Union(resolvedByAgentIds).ToList();

            if (allAgentIds.Any())
            {
                var agents = await db.Users
                    .Where(u => allAgentIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.DisplayName })
                    .ToListAsync(cancellationToken);

                foreach (var dto in conversationDtos)
                {
                    if (!string.IsNullOrEmpty(dto.CurrentAgentId))
                    {
                        var agent = agents.FirstOrDefault(a => a.Id == dto.CurrentAgentId);
                        dto.CurrentAgentName = agent?.DisplayName ?? "Unknown Agent";
                    }

                    if (!string.IsNullOrEmpty(dto.ResolvedByAgentId))
                    {
                        var agent = agents.FirstOrDefault(a => a.Id == dto.ResolvedByAgentId);
                        dto.ResolvedByAgentName = agent?.DisplayName ?? "Unknown Agent";
                    }
                }
            }

            var paginatedResult = new PaginatedData<ConversationDto>(
                conversationDtos,
                totalCount,
                request.Page,
                request.PageSize);

            _logger.LogInformation("Retrieved {Count} past conversations for agent {AgentId} out of {Total} total",
                conversationDtos.Count, request.AgentId, totalCount);

            return await Result<PaginatedData<ConversationDto>>.SuccessAsync(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving past conversations for agent {AgentId}: {@Filters}", request.AgentId, request);
            return await Result<PaginatedData<ConversationDto>>.FailureAsync($"Error retrieving past conversations: {ex.Message}");
        }
    }
}