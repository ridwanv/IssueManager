// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Interfaces.MultiTenant;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;
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
    private readonly IAutoAssignmentService _autoAssignmentService;
    private readonly ITenantService _tenantService;
    
    public EscalateConversationCommandHandler(
        IApplicationDbContextFactory dbContextFactory,
        IApplicationHubWrapper hubWrapper,
        IAutoAssignmentService autoAssignmentService,
        ITenantService tenantService)
    {
        _dbContextFactory = dbContextFactory;
        _hubWrapper = hubWrapper;
        _autoAssignmentService = autoAssignmentService;
        _tenantService = tenantService;
    }
    
    public async Task<Result<int>> Handle(EscalateConversationCommand request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
        
        // Check if conversation already exists
        var existingConversation = await db.Conversations
            .FirstOrDefaultAsync(c => c.ConversationReference == request.ConversationId, cancellationToken);
            
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
                ConversationReference = request.ConversationId,
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
            ConversationReference = existingConversation.ConversationReference,
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
        
        // Create escalation popup data
        var escalationPopup = new EscalationPopupDto
        {
            ConversationReference = existingConversation.ConversationReference,
            CustomerName = ExtractCustomerName(request.WhatsAppPhoneNumber),
            PhoneNumber = request.WhatsAppPhoneNumber ?? "Unknown",
            EscalationReason = request.Reason,
            Priority = DeterminePriority(request.Reason),
            EscalatedAt = existingConversation.EscalatedAt ?? DateTime.UtcNow,
            LastMessage = ExtractLastUserMessage(request.ConversationTranscript),
            MessageCount = CountMessagesFromTranscript(request.ConversationTranscript),
            ConversationDuration = existingConversation.Duration,
            ConversationSummary = TruncateTranscript(request.ConversationTranscript, 200)
        };
        
        // Notify agents via SignalR with both methods
        await _hubWrapper.BroadcastConversationEscalated(
            existingConversation.ConversationReference, 
            request.Reason, 
            request.WhatsAppPhoneNumber ?? "Unknown");
            
        // Send escalation popup to available agents
        await _hubWrapper.BroadcastEscalationPopupToAvailableAgents(escalationPopup);
        
        // Attempt auto-assignment if enabled for the tenant
        try
        {
            var tenantId = existingConversation.TenantId;
            
            if (await _autoAssignmentService.IsAutoAssignmentEnabledAsync(tenantId, cancellationToken))
            {
                var autoAssignResult = await _autoAssignmentService.AssignConversationAsync(
                    existingConversation.ConversationReference, 
                    tenantId, 
                    cancellationToken);
                
                // Auto-assignment is best-effort; we don't fail the escalation if it doesn't work
                if (autoAssignResult.Succeeded && autoAssignResult.Data.WasAssigned)
                {
                    // The assignment notification will be sent by the AssignAgentCommand handler
                    // so no additional SignalR notification needed here
                }
            }
        }
        catch (Exception)
        {
            // Auto-assignment failure should not prevent escalation success
            // Error is already logged in AutoAssignmentService
        }
            
        return Result<int>.Success(existingConversation.Id);
    }
    
    private static string ExtractCustomerName(string? phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
            return "Unknown Customer";
            
        // For now, just use phone number as name
        // TODO: Integrate with contact system to get actual names
        return phoneNumber.StartsWith("+") ? phoneNumber : $"+{phoneNumber}";
    }
    
    private static string? ExtractLastUserMessage(string? transcript)
    {
        if (string.IsNullOrEmpty(transcript))
            return null;
            
        // Try to extract the last user message from the transcript
        var lines = transcript.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        // Look for the last message from the user (not bot)
        for (int i = lines.Length - 1; i >= 0; i--)
        {
            var line = lines[i].Trim();
            if (line.StartsWith("User:", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("Customer:", StringComparison.OrdinalIgnoreCase) ||
                (!line.StartsWith("Bot:", StringComparison.OrdinalIgnoreCase) && 
                 !line.StartsWith("Agent:", StringComparison.OrdinalIgnoreCase) &&
                 !string.IsNullOrWhiteSpace(line)))
            {
                // Clean up the message (remove prefixes)
                var message = line
                    .Replace("User:", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("Customer:", "", StringComparison.OrdinalIgnoreCase)
                    .Trim();
                    
                return string.IsNullOrWhiteSpace(message) ? null : message;
            }
        }
        
        return null;
    }
    
    private static int DeterminePriority(string reason)
    {
        var lowerReason = reason?.ToLowerInvariant() ?? "";
        
        if (lowerReason.Contains("critical") || lowerReason.Contains("urgent") || lowerReason.Contains("emergency"))
            return 3; // Critical
        else if (lowerReason.Contains("high") || lowerReason.Contains("important") || lowerReason.Contains("priority"))
            return 2; // High
        else
            return 1; // Standard
    }
    
    private static int CountMessagesFromTranscript(string? transcript)
    {
        if (string.IsNullOrEmpty(transcript))
            return 0;
            
        // Simple message counting - count occurrences of typical message indicators
        var messageIndicators = new[] { "User:", "Bot:", "Agent:", "\n- " };
        return messageIndicators.Sum(indicator => transcript.Split(indicator, StringSplitOptions.RemoveEmptyEntries).Length - 1);
    }
    
    private static string? TruncateTranscript(string? transcript, int maxLength)
    {
        if (string.IsNullOrEmpty(transcript))
            return null;
            
        if (transcript.Length <= maxLength)
            return transcript;
            
        return transcript.Substring(0, maxLength) + "...";
    }
}