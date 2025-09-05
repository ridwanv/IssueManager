// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetAllConversations;

public record GetAllConversationsQuery(
    string? Status = null,
    string? UserId = null,
    string? SearchTerm = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = "LastActivityAt",
    bool SortDescending = true
) : IRequest<Result<PaginatedData<ConversationDto>>>;

public class GetAllConversationsQueryHandler : IRequestHandler<GetAllConversationsQuery, Result<PaginatedData<ConversationDto>>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllConversationsQueryHandler> _logger;

    public GetAllConversationsQueryHandler(
        IApplicationDbContextFactory dbContextFactory,
        IMapper mapper,
        ILogger<GetAllConversationsQueryHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PaginatedData<ConversationDto>>> Handle(GetAllConversationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

            var query = db.Conversations.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<ConversationStatus>(request.Status, true, out var status))
            {
                query = query.Where(c => c.Status == status);
            }

            if (!string.IsNullOrEmpty(request.UserId))
            {
                query = query.Where(c => c.UserId == request.UserId);
            }

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(c => 
                    c.UserName!.Contains(request.SearchTerm) ||
                    c.ConversationSummary!.Contains(request.SearchTerm) ||
                    c.EscalationReason!.Contains(request.SearchTerm) ||
                    c.WhatsAppPhoneNumber!.Contains(request.SearchTerm));
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(c => c.Created >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(c => c.Created <= request.EndDate.Value);
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
                _ => request.SortDescending 
                    ? query.OrderByDescending(c => c.LastActivityAt)
                    : query.OrderBy(c => c.LastActivityAt)
            };

            // Get total count for pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var conversations = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var conversationDtos = _mapper.Map<List<ConversationDto>>(conversations);

            // Get agent names for conversations that have current agents
            var agentIds = conversationDtos
                .Where(c => !string.IsNullOrEmpty(c.CurrentAgentId))
                .Select(c => c.CurrentAgentId!)
                .Distinct()
                .ToList();

            if (agentIds.Any())
            {
                var agents = await db.Users
                    .Where(u => agentIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.DisplayName })
                    .ToListAsync(cancellationToken);

                foreach (var dto in conversationDtos)
                {
                    if (!string.IsNullOrEmpty(dto.CurrentAgentId))
                    {
                        var agent = agents.FirstOrDefault(a => a.Id == dto.CurrentAgentId);
                        dto.CurrentAgentName = agent?.DisplayName ?? "Unknown Agent";
                    }
                }
            }

            var paginatedResult = new PaginatedData<ConversationDto>(
                conversationDtos,
                totalCount,
                request.Page,
                request.PageSize);

            _logger.LogInformation("Retrieved {Count} conversations out of {Total} total conversations",
                conversationDtos.Count, totalCount);

            return await Result<PaginatedData<ConversationDto>>.SuccessAsync(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversations with filters: {@Filters}", request);
            return await Result<PaginatedData<ConversationDto>>.FailureAsync($"Error retrieving conversations: {ex.Message}");
        }
    }
}
