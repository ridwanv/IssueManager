using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using CleanArchitecture.Blazor.Application.Common.Interfaces;

namespace CleanArchitecture.Blazor.Server.UI.Hubs;

public class ServerHubWrapper : IApplicationHubWrapper
{
    private readonly IHubContext<ServerHub, ISignalRHub> _hubContext;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ServerHubWrapper(IHubContext<ServerHub, ISignalRHub> hubContext, IServiceScopeFactory serviceScopeFactory)
    {
        _hubContext = hubContext;
        _serviceScopeFactory = serviceScopeFactory;
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
    public async Task BroadcastConversationEscalated(string conversationId, string reason, string customerPhoneNumber)
    {
        // Use the enhanced notification method with priority and timestamp
        var priority = DeterminePriority(reason);
        var escalatedAt = DateTime.UtcNow;
        
        // Call the enhanced escalation notification methods directly
        await _hubContext.Clients.Group("Agents").EscalationNotification(conversationId, reason, customerPhoneNumber, priority, escalatedAt);
        
        // Send targeted notifications to agents based on their preferences
        await SendTargetedNotifications(conversationId, reason, customerPhoneNumber, priority, escalatedAt);
        
        // Also broadcast the old method for backward compatibility
        await _hubContext.Clients.Group("Agents").ConversationEscalated(conversationId, reason, customerPhoneNumber).ConfigureAwait(false);
    }
    
    private async Task SendTargetedNotifications(string conversationId, string reason, string customerPhoneNumber, int priority, DateTime escalatedAt)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IApplicationDbContextFactory>();
            
            await using var db = await dbContextFactory.CreateAsync();
            
            var filteredAgents = await db.AgentNotificationPreferences
                .Include(p => p.ApplicationUser)
                .Where(p => 
                    (priority == 1 && p.NotifyOnStandardPriority) ||
                    (priority == 2 && p.NotifyOnHighPriority) ||
                    (priority == 3 && p.NotifyOnCriticalPriority))
                .Where(p => p.EnableBrowserNotifications || p.EnableAudioAlerts || p.EnableEmailNotifications)
                .Select(p => p.ApplicationUserId)
                .ToListAsync();
            
            // Send targeted notifications to filtered agents
            foreach (var agentUserId in filteredAgents)
            {
                await _hubContext.Clients.User(agentUserId).TargetedEscalationNotification(conversationId, reason, customerPhoneNumber, priority, escalatedAt);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the entire escalation
            Console.WriteLine($"Error sending targeted notifications: {ex.Message}");
        }
    }
    
    private int DeterminePriority(string reason)
    {
        // Simple priority logic based on keywords in reason
        var lowerReason = reason?.ToLowerInvariant() ?? "";
        
        if (lowerReason.Contains("critical") || lowerReason.Contains("urgent") || lowerReason.Contains("emergency"))
            return 3; // Critical
        else if (lowerReason.Contains("high") || lowerReason.Contains("important") || lowerReason.Contains("priority"))
            return 2; // High
        else
            return 1; // Standard
    }

    public async Task BroadcastConversationAssigned(string conversationId, string agentId, string agentName)
    {
        await _hubContext.Clients.All.ConversationAssigned(conversationId, agentId, agentName).ConfigureAwait(false);
    }

    public async Task BroadcastConversationTransferred(string conversationId, string? fromAgentId, string toAgentId, string fromAgentName, string toAgentName)
    {
        // Notify all clients about the transfer
        await _hubContext.Clients.All.ConversationTransferred(conversationId, fromAgentId, toAgentId, fromAgentName, toAgentName).ConfigureAwait(false);
        
        // Send targeted notifications to the agents involved
        if (!string.IsNullOrEmpty(fromAgentId))
        {
            await _hubContext.Clients.User(fromAgentId).ConversationTransferredFrom(conversationId, toAgentId, toAgentName);
        }
        
        await _hubContext.Clients.User(toAgentId).ConversationTransferredTo(conversationId, fromAgentId, fromAgentName);
    }

    public async Task BroadcastConversationCompleted(string conversationId, string agentId)
    {
        await _hubContext.Clients.All.ConversationCompleted(conversationId, agentId).ConfigureAwait(false);
    }

    public async Task BroadcastAgentStatusChanged(string agentId, string status)
    {
        await _hubContext.Clients.Group("Agents").AgentStatusChanged(agentId, status).ConfigureAwait(false);
    }

    public async Task BroadcastNewConversationMessage(string conversationId, string from, string message, bool isFromAgent)
    {
        // Create a proper ConversationMessageDto for the NewMessageReceived event
        var messageDto = new
        {
            ConversationId = conversationId,
            BotFrameworkConversationId = conversationId,
            Content = message,
            Role = isFromAgent ? "agent" : "user",
            UserName = from,
            Timestamp = DateTime.UtcNow
        };

        // Send NewMessageReceived to conversation group (for ConversationDetail page)
        await _hubContext.Clients.Group($"Conversation_{conversationId}").NewMessageReceived(messageDto).ConfigureAwait(false);
        
        // Also send legacy NewConversationMessage to all clients (for global notification components)
        await _hubContext.Clients.All.NewConversationMessage(conversationId, from, message, isFromAgent).ConfigureAwait(false);
    }

    public async Task BroadcastNewMessageToConversationGroup(string conversationId, object messageDto)
    {
        Console.WriteLine($"[ServerHubWrapper] BroadcastNewMessageToConversationGroup - ConversationId: {conversationId}, Group: Conversation_{conversationId}");
        
        // Send the proper ConversationMessageDto to the conversation group
        await _hubContext.Clients.Group($"Conversation_{conversationId}").NewMessageReceived(messageDto).ConfigureAwait(false);
        
        Console.WriteLine($"[ServerHubWrapper] Sent NewMessageReceived to group Conversation_{conversationId}");
    }

    // Multi-agent popup methods
    public async Task BroadcastEscalationPopupToAvailableAgents(object escalationPopupDto)
    {
        try
        {
            // Get available agents with capacity
            var availableAgents = await GetAvailableAgentsWithCapacity();
            
            Console.WriteLine($"🔍 DEBUG: Found {availableAgents.Count} available agents with capacity");
            
            // Send popup to all available agents
            foreach (var agentId in availableAgents)
            {
                Console.WriteLine($"🔍 DEBUG: Sending popup to agent {agentId}");
                await _hubContext.Clients.User(agentId).ReceiveEscalationPopup(escalationPopupDto);
            }
            
            // FALLBACK: Also send to ALL agents in the group (for debugging)
            Console.WriteLine("🔍 DEBUG: Also broadcasting to ALL agents in group as fallback");
            await _hubContext.Clients.Group("Agents").ReceiveEscalationPopup(escalationPopupDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error broadcasting escalation popup: {ex.Message}");
        }
    }

    public async Task NotifyEscalationAccepted(string conversationId, string acceptingAgentId)
    {
        try
        {
            // Send dismiss signal to all agents except the one who accepted
            await _hubContext.Clients.GroupExcept("Agents", acceptingAgentId).DismissEscalationPopup(conversationId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error notifying escalation acceptance: {ex.Message}");
        }
    }
    
    private async Task<List<string>> GetAvailableAgentsWithCapacity()
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IApplicationDbContextFactory>();
            
            await using var db = await dbContextFactory.CreateAsync();
            
            var availableAgents = await db.Agents
                .Include(a => a.ApplicationUser)
                .Where(a => 
                    a.Status == CleanArchitecture.Blazor.Domain.Enums.AgentStatus.Available &&
                    a.ActiveConversationCount < a.MaxConcurrentConversations)
                .Select(a => a.ApplicationUserId)
                .ToListAsync();
            
            Console.WriteLine($"🔍 DEBUG: GetAvailableAgentsWithCapacity found {availableAgents.Count} agents");
            foreach (var agentId in availableAgents)
            {
                Console.WriteLine($"🔍 DEBUG: Available agent ID: {agentId}");
            }
            
            return availableAgents;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔍 DEBUG: GetAvailableAgentsWithCapacity failed: {ex.Message}");
            // Fallback to all agents in group if query fails
            return new List<string>();
        }
    }


}