using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace CleanArchitecture.Blazor.Server.UI.Components.Conversations;

public partial class ConversationInteractionComponent : ComponentBase
{
    [Parameter] public ConversationDetailsDto? ConversationDetails { get; set; }
    [Parameter] public EventCallback<string> OnMessageSent { get; set; }
    [Parameter] public bool SendingMessage { get; set; }
    
    [Inject] public IStringLocalizer<ConversationInteractionComponent> L { get; set; } = default!;

    private async Task HandleMessageSent(string message)
    {
        if (OnMessageSent.HasDelegate)
        {
            await OnMessageSent.InvokeAsync(message);
        }
    }
}