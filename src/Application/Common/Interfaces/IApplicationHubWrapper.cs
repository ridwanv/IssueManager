namespace CleanArchitecture.Blazor.Application.Common.Interfaces;


public interface IApplicationHubWrapper
{
    Task JobStarted(int id,string message);
    Task JobCompleted(int id,string message);
    
    // Agent escalation methods
    Task BroadcastConversationEscalated(int conversationId, string reason, string customerPhoneNumber);
    Task BroadcastConversationAssigned(int conversationId, string agentId, string agentName);
    Task BroadcastConversationCompleted(int conversationId, string agentId);
    Task BroadcastAgentStatusChanged(string agentId, string status);
    Task BroadcastNewConversationMessage(int conversationId, string from, string message, bool isFromAgent);
}