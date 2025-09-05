// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Commands.EscalateConversation;

public record EscalateConversationCommand(
    string ConversationId,
    string Reason,
    string? ConversationTranscript = null,
    string? WhatsAppPhoneNumber = null
) : ICacheInvalidatorRequest<Result<int>>
{
    public string CacheKey => $"conversations-{ConversationId}";
    public IEnumerable<string>? Tags => new[] { "conversations" };
    public CancellationToken CancellationToken { get; set; }
}

public class EscalateConversationCommandHandler : IRequestHandler<EscalateConversationCommand, Result<int>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IApplicationHubWrapper _hubWrapper;
    
    public EscalateConversationCommandHandler(
        IApplicationDbContextFactory dbContextFactory,
        IApplicationHubWrapper hubWrapper)
    {
        _dbContextFactory = dbContextFactory;
        _hubWrapper = hubWrapper;
    }
    
    public async Task<Result<int>> Handle(EscalateConversationCommand request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
        
        // Check if conversation already exists
        var existingConversation = await db.Conversations
            .FirstOrDefaultAsync(c => c.ConversationId == request.ConversationId, cancellationToken);
            
        var isNewConversation = false;
        if (existingConversation != null)
        {
            // Update existing conversation to escalated state
            existingConversation.Mode = ConversationMode.Escalating;
            existingConversation.EscalatedAt = DateTime.UtcNow;
            existingConversation.EscalationReason = request.Reason;
            existingConversation.LastActivityAt = DateTime.UtcNow;
            
            if (!string.IsNullOrEmpty(request.WhatsAppPhoneNumber))
                existingConversation.WhatsAppPhoneNumber = request.WhatsAppPhoneNumber;
        }
        else
        {
            // Create new conversation in escalated state
            var conversation = new Conversation
            {
                ConversationId = request.ConversationId,
                WhatsAppPhoneNumber = request.WhatsAppPhoneNumber,
                Status = ConversationStatus.Active,
                Mode = ConversationMode.Escalating,
                EscalatedAt = DateTime.UtcNow,
                EscalationReason = request.Reason,
                LastActivityAt = DateTime.UtcNow,
                TenantId = "default" // TODO: Get from context
            };
            
            db.Conversations.Add(conversation);
            existingConversation = conversation;
            isNewConversation = true;
        }
        
        // If new conversation, save to get the generated Id for FK
        if (isNewConversation)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        
        // Create escalation handoff record
        var handoff = new ConversationHandoff
        {
            ConversationId = existingConversation.Id,
            HandoffType = HandoffType.BotToHuman,
            FromParticipantType = ParticipantType.Bot,
            ToParticipantType = ParticipantType.Agent,
            Reason = request.Reason,
            ConversationTranscript = request.ConversationTranscript,
            Status = HandoffStatus.Initiated,
            InitiatedAt = DateTime.UtcNow,
            TenantId = existingConversation.TenantId
        };
        
        db.ConversationHandoffs.Add(handoff);
        
        await db.SaveChangesAsync(cancellationToken);
        
        // Notify agents via SignalR
        await _hubWrapper.BroadcastConversationEscalated(
            existingConversation.Id, 
            request.Reason, 
            request.WhatsAppPhoneNumber ?? "Unknown");
            
        return Result<int>.Success(existingConversation.Id);
    }
}