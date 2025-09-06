namespace CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;

public class TypingIndicatorDto
{
    public string ConversationId { get; set; } = string.Empty;
    public bool IsTyping { get; set; }
    public DateTime Timestamp { get; set; }
    public string? UserName { get; set; }
}