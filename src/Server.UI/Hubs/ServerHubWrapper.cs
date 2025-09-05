using Microsoft.AspNetCore.SignalR;

namespace CleanArchitecture.Blazor.Server.UI.Hubs;

public class ServerHubWrapper : IApplicationHubWrapper
{
    private readonly IHubContext<ServerHub, ISignalRHub> _hubContext;

    public ServerHubWrapper(IHubContext<ServerHub, ISignalRHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task JobStarted(int id, string message)
    {
        await _hubContext.Clients.All.Start(id,message).ConfigureAwait(false);
    }

    public async Task JobCompleted(int id, string message)
    {
        await _hubContext.Clients.All.Completed(id,message).ConfigureAwait(false); 
    }

    // Agent escalation methods
    public async Task BroadcastConversationEscalated(int conversationId, string reason, string customerPhoneNumber)
    {
        await _hubContext.Clients.Group("Agents").ConversationEscalated(conversationId, reason, customerPhoneNumber).ConfigureAwait(false);
    }

    public async Task BroadcastConversationAssigned(int conversationId, string agentId, string agentName)
    {
        await _hubContext.Clients.All.ConversationAssigned(conversationId, agentId, agentName).ConfigureAwait(false);
    }

    public async Task BroadcastConversationCompleted(int conversationId, string agentId)
    {
        await _hubContext.Clients.All.ConversationCompleted(conversationId, agentId).ConfigureAwait(false);
    }

    public async Task BroadcastAgentStatusChanged(string agentId, string status)
    {
        await _hubContext.Clients.Group("Agents").AgentStatusChanged(agentId, status).ConfigureAwait(false);
    }

    public async Task BroadcastNewConversationMessage(int conversationId, string from, string message, bool isFromAgent)
    {
        await _hubContext.Clients.All.NewConversationMessage(conversationId, from, message, isFromAgent).ConfigureAwait(false);
    }
}