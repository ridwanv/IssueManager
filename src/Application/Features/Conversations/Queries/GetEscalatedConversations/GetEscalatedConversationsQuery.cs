// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetEscalatedConversations;

public record GetEscalatedConversationsQuery() : ICacheableRequest<Result<List<ConversationDto>>>
{
    public string CacheKey => "escalated-conversations";
    public IEnumerable<string>? Tags => new[] { "conversations" };
    public TimeSpan? Expiry => TimeSpan.FromMinutes(1); // Short cache for real-time data
}

public class GetEscalatedConversationsQueryHandler : IRequestHandler<GetEscalatedConversationsQuery, Result<List<ConversationDto>>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    
    public GetEscalatedConversationsQueryHandler(IApplicationDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }
    
    public async Task<Result<List<ConversationDto>>> Handle(GetEscalatedConversationsQuery request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
        
        var conversations = await db.Conversations
            .Where(c => c.Status == ConversationStatus.Active && 
                       (c.Mode == ConversationMode.Escalating || c.Mode == ConversationMode.Human))
            .OrderBy(c => c.EscalatedAt)
            .Select(c => new ConversationDto
            {
                Id = c.Id,
                ConversationId = c.ConversationId,
                WhatsAppPhoneNumber = c.WhatsAppPhoneNumber,
                Status = c.Status,
                Mode = c.Mode,
                CurrentAgentId = c.CurrentAgentId,
                CurrentAgentName = null, // Will be populated below
                EscalatedAt = c.EscalatedAt,
                CompletedAt = c.CompletedAt,
                EscalationReason = c.EscalationReason,
                ConversationSummary = c.ConversationSummary,
                MessageCount = c.MessageCount,
                LastActivityAt = c.LastActivityAt,
                Created = c.Created ?? DateTime.UtcNow,
                TenantId = c.TenantId
            })
            .ToListAsync(cancellationToken);
            
        // Get agent names for assigned conversations
        var agentIds = conversations
            .Where(c => !string.IsNullOrEmpty(c.CurrentAgentId))
            .Select(c => c.CurrentAgentId!)
            .Distinct()
            .ToList();
            
        if (agentIds.Any())
        {
            var agentNames = await db.Users
                .Where(u => agentIds.Contains(u.Id))
                .Select(u => new { u.Id, Name = u.DisplayName ?? u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.Name, cancellationToken);
                
            foreach (var conversation in conversations)
            {
                if (!string.IsNullOrEmpty(conversation.CurrentAgentId) && 
                    agentNames.TryGetValue(conversation.CurrentAgentId, out var agentName))
                {
                    conversation.CurrentAgentName = agentName;
                }
            }
        }
        
        return Result<List<ConversationDto>>.Success(conversations);
    }
}