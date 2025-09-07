using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace CleanArchitecture.Blazor.Server.UI.Components.Conversations;

public partial class ConversationMessagesComponent : ComponentBase
{
    [Parameter] public ConversationDetailsDto? ConversationDetails { get; set; }
    [Parameter] public bool CustomerTyping { get; set; }
    
    [Inject] public IStringLocalizer<ConversationMessagesComponent> L { get; set; } = default!;

    private string GetMessageContainerClass(string role)
    {
        return role == "user" ? "d-flex justify-end" : "d-flex justify-start";
    }

    private string GetMessageBubbleClass(string role)
    {
        var baseClass = "message-bubble d-flex flex-column pa-3 ma-2";
        return role == "user" ? $"{baseClass} user-message" : $"{baseClass} bot-message";
    }

    private string GetMessageBubbleStyle(string role)
    {
        if (role == "user")
        {
            return "background-color: var(--mud-palette-primary); color: white; border-radius: 18px 18px 4px 18px; max-width: 70%;";
        }
        return "background-color: var(--mud-palette-surface-variant); border-radius: 18px 18px 18px 4px; max-width: 70%;";
    }

    private Color GetMessageAvatarColor(string role)
    {
        return role == "user" ? Color.Primary : Color.Secondary;
    }

    private string GetMessageAvatarIcon(string role)
    {
        return role == "user" ? Icons.Material.Filled.Person : Icons.Material.Filled.SmartToy;
    }

    private string GetRoleDisplayName(string role)
    {
        return role switch
        {
            "user" => L["Customer"],
            "assistant" => L["AI Assistant"],
            "system" => L["System"],
            _ => L["Bot"]
        };
    }

    private Color GetRoleChipColor(string role)
    {
        return role switch
        {
            "assistant" => Color.Info,
            "system" => Color.Warning,
            _ => Color.Secondary
        };
    }
}