namespace CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;

public class ConversationViewerDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? ProfilePictureDataUrl { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsCurrentUser { get; set; }
}