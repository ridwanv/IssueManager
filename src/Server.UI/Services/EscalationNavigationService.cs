using Microsoft.AspNetCore.Components;

namespace IssueManager.Server.UI.Services;

public class EscalationNavigationService
{
    private readonly NavigationManager _navigationManager;

    public EscalationNavigationService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public void NavigateToConversation(int conversationId)
    {
        var url = $"/agent/conversations/{conversationId}";
        _navigationManager.NavigateTo(url);
    }

    public void NavigateToAgentDashboard()
    {
        _navigationManager.NavigateTo("/conversations/agent-dashboard");
    }

    public void NavigateToConversationList()
    {
        _navigationManager.NavigateTo("/agent/conversations");
    }

    public bool IsCurrentConversation(int conversationId)
    {
        var currentUrl = _navigationManager.Uri;
        return currentUrl.Contains($"/agent/conversations/{conversationId}");
    }
}