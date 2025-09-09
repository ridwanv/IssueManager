namespace CleanArchitecture.Blazor.Application.Common.Interfaces;


public interface IApplicationHubWrapper
{
    Task JobStarted(int id,string message);
    Task JobCompleted(int id,string message);
    
    // Agent escalation methods
    Task BroadcastConversationEscalated(string conversationId, string reason, string customerPhoneNumber);
    Task BroadcastConversationAssigned(string conversationId, string agentId, string agentName);
    Task BroadcastConversationCompleted(string conversationId, string agentId);
    Task BroadcastAgentStatusChanged(string agentId, string status);
    Task BroadcastNewConversationMessage(string conversationId, string from, string message, bool isFromAgent);
    Task BroadcastNewMessageToConversationGroup(string conversationId, object messageDto);
    
    // Multi-agent popup methods
    Task BroadcastEscalationPopupToAvailableAgents(object escalationPopupDto);
    Task NotifyEscalationAccepted(string conversationId, string acceptingAgentId);
}