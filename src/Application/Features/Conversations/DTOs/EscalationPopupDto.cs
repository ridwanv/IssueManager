using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;

public class EscalationPopupDto
{
    public string ConversationReference { get; set; }
    public required string CustomerName { get; set; }
    public required string PhoneNumber { get; set; }
    public required string EscalationReason { get; set; }
    public int Priority { get; set; }
    public DateTime EscalatedAt { get; set; }
    public string? LastMessage { get; set; }
    public int MessageCount { get; set; }
    public TimeSpan ConversationDuration { get; set; }
    public string? ConversationSummary { get; set; }
}