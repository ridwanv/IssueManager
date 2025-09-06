// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;

public record ConversationDto
{
    public int Id { get; set; }
    public required string ConversationReference { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? WhatsAppPhoneNumber { get; set; }
    public ConversationStatus Status { get; set; }
    public ConversationMode Mode { get; set; }
    public int Priority { get; set; } = 1; // 1=Standard, 2=High, 3=Critical
    public string? CurrentAgentId { get; set; }
    public string? CurrentAgentName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EscalatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? EscalationReason { get; set; }
    public string? ConversationSummary { get; set; }
    public int MessageCount { get; set; }
    public DateTime LastActivityAt { get; set; }
    public DateTime Created { get; set; }
    public string? ThreadId { get; set; }
    public int MaxTurns { get; set; }
    public string TenantId { get; set; } = default!;
    
    // Computed properties
    public bool IsEscalated => Mode == ConversationMode.Human || Mode == ConversationMode.Escalating;
    public bool IsActive => Status == ConversationStatus.Active;
    public TimeSpan Duration => (CompletedAt ?? DateTime.UtcNow).Subtract(Created);
    public string StatusText => Status.ToString();
    public string ModeText => Mode.ToString();
    public string DurationText => Duration.ToString(@"hh\:mm\:ss");
    public string PriorityText => Priority switch
    {
        1 => "Standard",
        2 => "High",
        3 => "Critical",
        _ => "Standard"
    };
    public bool IsPriority => Priority > 1;
    
    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Conversation, ConversationDto>()
                .ForMember(dest => dest.CurrentAgentName, opt => opt.Ignore()); // Will be set separately
        }
    }
}

public record ConversationDetailsDto
{
    public required ConversationDto Conversation { get; set; }
    public List<ConversationMessageDto> Messages { get; set; } = new();
    public List<ConversationParticipantDto> Participants { get; set; } = new();
    public List<ConversationHandoffDto> HandoffHistory { get; set; } = new();
}

public record ConversationParticipantDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public ParticipantType Type { get; set; }
    public string? ParticipantId { get; set; }
    public string? ParticipantName { get; set; }
    public string? WhatsAppPhoneNumber { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public bool IsActive { get; set; }
    public TimeSpan? ParticipationDuration => LeftAt?.Subtract(JoinedAt);
}

public record ConversationHandoffDto
{
    public int Id { get; set; }
    public string ConversationId { get; set; }
    public HandoffType HandoffType { get; set; }
    public ParticipantType FromParticipantType { get; set; }
    public ParticipantType ToParticipantType { get; set; }
    public string? FromAgentId { get; set; }
    public string? ToAgentId { get; set; }
    public string Reason { get; set; } = default!;
    public string? ConversationTranscript { get; set; }
    public string? ContextData { get; set; }
    public HandoffStatus Status { get; set; }
    public DateTime InitiatedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public TimeSpan? HandoffDuration => CompletedAt?.Subtract(InitiatedAt);
    public bool IsCompleted => Status == HandoffStatus.Completed;
}