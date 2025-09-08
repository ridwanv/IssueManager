// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;
using CleanArchitecture.Blazor.Domain.Identity;
using CleanArchitecture.Blazor.Application.Features.Issues.DTOs;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CleanArchitecture.Blazor.Application.Common.Interfaces;

namespace CleanArchitecture.Blazor.Server.UI.Hubs;

[Authorize(AuthenticationSchemes = "Identity.Application")]
public class ServerHub : Hub<ISignalRHub>
{
    private sealed record ConnectionUser(string UserId, string UserName);
    private static readonly ConcurrentDictionary<string, ConnectionUser> OnlineUsers = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> ComponentUsers = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, HashSet<string>> AgentGroups = new(StringComparer.Ordinal); // userId -> group names
    private readonly IServiceScopeFactory _scopeFactory;
    public ServerHub(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var username = Context.User?.Identity?.Name ?? string.Empty;
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? Context.User?.FindFirst("sub")?.Value
                     ?? username;
        // Notify all clients if this is a new user connecting.
        if (!OnlineUsers.Any(x => string.Equals(x.Value.UserId, userId, StringComparison.Ordinal)))
        {
            await Clients.All.Connect(connectionId, username).ConfigureAwait(false);
        }
        if (!OnlineUsers.ContainsKey(connectionId))
        {
            OnlineUsers.TryAdd(connectionId, new ConnectionUser(userId, username));
        }
        await base.OnConnectedAsync().ConfigureAwait(false); 
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        // Remove the connection and check if it was the last one for this user.
        if (OnlineUsers.TryRemove(connectionId, out var connectionUser))
        {
            if (!OnlineUsers.Any(x => string.Equals(x.Value.UserId, connectionUser.UserId, StringComparison.Ordinal)))
            {
                await Clients.All.Disconnect(connectionId, connectionUser.UserName).ConfigureAwait(false);
            }    
        }
        await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
    }

    public async Task SendMessage(string message)
    {
        var username = Context.User?.Identity?.Name ?? string.Empty;
        await Clients.All.SendMessage(username, message).ConfigureAwait(false);
    }

    public async Task SendPrivateMessage(string to, string message)
    {
        var username = Context.User?.Identity?.Name ?? string.Empty;
        await Clients.User(to).SendPrivateMessage(username, to, message).ConfigureAwait(false);
    }

    public async Task SendNotification(string message)
    {
        await Clients.All.SendNotification(message).ConfigureAwait(false);
    }

    public async Task Completed(int id,string message)
    {
        await Clients.All.Completed(id,message).ConfigureAwait(false);
    }

    // Client -> Server: notify open/close of a specific page component
    public async Task NotifyPageComponentOpen(string pageComponent)
    {
        var username = Context.User?.Identity?.Name ?? string.Empty;
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? Context.User?.FindFirst("sub")?.Value
                     ?? username;
        var users = ComponentUsers.GetOrAdd(pageComponent, _ => new ConcurrentDictionary<string, string>(StringComparer.Ordinal));
        // Send existing users of this component to the caller first
        foreach (var kvp in users)
        {
            await Clients.Caller.PageComponentOpened(pageComponent, kvp.Key, kvp.Value).ConfigureAwait(false);
        }
        users[userId] = username;
        // Notify all clients that this user opened the component
        await Clients.All.PageComponentOpened(pageComponent, userId, username).ConfigureAwait(false);
    }

    public async Task NotifyPageComponentClose(string pageComponent)
    {
        var username = Context.User?.Identity?.Name ?? string.Empty;
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? Context.User?.FindFirst("sub")?.Value
                     ?? username;
        if (ComponentUsers.TryGetValue(pageComponent, out var users))
        {
            users.TryRemove(userId, out _);
            if (users.IsEmpty)
            {
                ComponentUsers.TryRemove(pageComponent, out _);
            }
        }
        await Clients.All.PageComponentClosed(pageComponent, userId, username).ConfigureAwait(false);
    }

    // Client -> Server: returns a snapshot of distinct online users with profile data
    public async Task<List<UserContext>> GetOnlineUsers()
    {
        var distinctUsers = OnlineUsers.Values
            .GroupBy(v => v.UserId, StringComparer.Ordinal)
            .Select(g => g.First())
            .ToList();

        using var scope = _scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var result = new List<UserContext>(distinctUsers.Count);
        foreach (var cu in distinctUsers.OrderBy(u => u.UserName, StringComparer.Ordinal))
        {
            var appUser = await userManager.FindByIdAsync(cu.UserId).ConfigureAwait(false);
            result.Add(new UserContext(
                UserId: cu.UserId,
                UserName: cu.UserName,
                DisplayName: appUser?.DisplayName,
                TenantId: appUser?.TenantId,
                Email: appUser?.Email,
                Roles: null,
                ProfilePictureDataUrl: appUser?.ProfilePictureDataUrl,
                SuperiorId: appUser?.SuperiorId
            ));
        }
        return result;
    }

    // Issue real-time update methods
    public async Task BroadcastIssueCreated(IssueListDto issue)
    {
        await Clients.All.IssueCreated(issue).ConfigureAwait(false);
    }

    public async Task BroadcastIssueUpdated(IssueListDto issue)
    {
        await Clients.All.IssueUpdated(issue).ConfigureAwait(false);
    }

    public async Task BroadcastIssueStatusChanged(Guid issueId, IssueStatus newStatus)
    {
        await Clients.All.IssueStatusChanged(issueId, newStatus).ConfigureAwait(false);
    }

    public async Task BroadcastIssueListUpdated()
    {
        await Clients.All.IssueListUpdated().ConfigureAwait(false);
    }

    // Agent escalation methods
    public async Task JoinAgentGroup()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, "Agents");
        
        // Track agent group membership
        AgentGroups.AddOrUpdate(userId, 
            new HashSet<string> { "Agents" },
            (key, existing) => { existing.Add("Agents"); return existing; });
    }

    public async Task LeaveAgentGroup()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Agents");
        
        // Update agent group membership
        if (AgentGroups.TryGetValue(userId, out var groups))
        {
            groups.Remove("Agents");
        }
    }

    public async Task BroadcastConversationEscalated(string conversationId, string reason, string customerPhoneNumber)
    {
        await Clients.Group("Agents").ConversationEscalated(conversationId, reason, customerPhoneNumber).ConfigureAwait(false);
    }

    // New methods for persistent escalation notifications
    public async Task BroadcastEscalationPersistentNotification(string conversationId, string reason, string customerPhoneNumber, int priority, DateTime escalatedAt)
    {
        Console.WriteLine($"[ServerHub] Broadcasting persistent escalation notification: {conversationId}, Priority: {priority}");
        await Clients.Group("Agents").EscalationPersistentNotification(conversationId, reason, customerPhoneNumber, priority, escalatedAt).ConfigureAwait(false);
    }

    public async Task BroadcastEscalationAccepted(string conversationId)
    {
        Console.WriteLine($"[ServerHub] Broadcasting escalation accepted: {conversationId}");
        await Clients.Group("Agents").EscalationAccepted(conversationId).ConfigureAwait(false);
    }

    public async Task BroadcastEscalationIgnored(string conversationId, string agentId)
    {
        Console.WriteLine($"[ServerHub] Broadcasting escalation ignored: {conversationId} by agent {agentId}");
        await Clients.Group("Agents").EscalationIgnored(conversationId, agentId).ConfigureAwait(false);
    }

    // Enhanced notification methods for improved agent dashboard
    public async Task BroadcastEscalationNotification(string conversationId, string reason, string customerPhoneNumber, int priority = 1, DateTime? escalatedAt = null)
    {
        var escalationTime = escalatedAt ?? DateTime.UtcNow;
        
        // Get filtered agents based on preferences
        var targetedAgents = await GetFilteredAgentsForNotification(priority);
        
        // Send to all agents first (for dashboard updates)
        await Clients.Group("Agents").EscalationNotification(conversationId, reason, customerPhoneNumber, priority, escalationTime).ConfigureAwait(false);
        
        // Send targeted notifications with preference filtering
        foreach (var agentUserId in targetedAgents)
        {
            await Clients.User(agentUserId).TargetedEscalationNotification(conversationId, reason, customerPhoneNumber, priority, escalationTime).ConfigureAwait(false);
        }
    }
    
    private async Task<List<string>> GetFilteredAgentsForNotification(int priority)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
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
            
            return filteredAgents;
        }
        catch (Exception)
        {
            // Fallback to all agents if preferences query fails
            return new List<string>();
        }
    }

    public async Task BroadcastAgentWorkloadUpdated(int agentId, int activeCount, int maxCount, bool isAvailable)
    {
        await Clients.Group("Agents").AgentWorkloadUpdated(agentId, activeCount, maxCount, isAvailable).ConfigureAwait(false);
    }

    public async Task BroadcastEscalationQueueUpdated(int queueCount)
    {
        await Clients.Group("Agents").EscalationQueueUpdated(queueCount).ConfigureAwait(false);
    }

    public async Task BroadcastAgentAvailabilityChanged(int agentId, string status, bool isAvailable, string agentName)
    {
        await Clients.Group("Agents").AgentAvailabilityChanged(agentId, status, isAvailable, agentName).ConfigureAwait(false);
    }

    public async Task BroadcastConversationAssigned(string conversationId, string agentId, string agentName)
    {
        await Clients.All.ConversationAssigned(conversationId, agentId, agentName).ConfigureAwait(false);
    }

    public async Task BroadcastConversationCompleted(string conversationId, string agentId)
    {
        await Clients.All.ConversationCompleted(conversationId, agentId).ConfigureAwait(false);
    }

    public async Task BroadcastAgentStatusChanged(string agentId, string status)
    {
        await Clients.Group("Agents").AgentStatusChanged(agentId, status).ConfigureAwait(false);
    }

    public async Task BroadcastNewConversationMessage(string conversationId, string from, string message, bool isFromAgent)
    {
        await Clients.All.NewConversationMessage(conversationId, from, message, isFromAgent).ConfigureAwait(false);
    }

    // Agent-specific methods for conversation management
    public async Task AcceptConversation(string conversationId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.Identity?.Name ?? "Agent";
        
        if (string.IsNullOrEmpty(userId)) return;

        await BroadcastConversationAssigned(conversationId, userId, userName);
    }

    // Issue-Conversation Integration Methods
    public async Task JoinIssueConversationGroup(Guid issueId, string conversationId)
    {
        var groupName = $"Issue-{issueId}-Conversation-{conversationId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.Identity?.Name ?? "Agent";
        
        if (!string.IsNullOrEmpty(userId))
        {
            // Track agent group membership
            AgentGroups.AddOrUpdate(userId, 
                new HashSet<string> { groupName },
                (key, existing) => { existing.Add(groupName); return existing; });
            
            // Notify group about agent joining
            await Clients.Group(groupName).AgentJoinedIssueConversation(issueId, userId, userName);
        }
    }

    public async Task LeaveIssueConversationGroup(Guid issueId, string conversationId)
    {
        var groupName = $"Issue-{issueId}-Conversation-{conversationId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.Identity?.Name ?? "Agent";
        
        if (!string.IsNullOrEmpty(userId))
        {
            // Update agent group membership
            if (AgentGroups.TryGetValue(userId, out var groups))
            {
                groups.Remove(groupName);
            }
            
            // Notify group about agent leaving
            await Clients.Group(groupName).AgentLeftIssueConversation(issueId, userId, userName);
        }
    }


    public async Task BroadcastIssueConversationMessageReceived(Guid issueId, string conversationId, object message)
    {
        var groupName = $"Issue-{issueId}-Conversation-{conversationId}";
        await Clients.Group(groupName).IssueConversationMessageReceived(issueId, message);
    }

    public async Task SendConversationMessage(string conversationId, string message)
    {
        var userName = Context.User?.Identity?.Name ?? "Agent";
        await BroadcastNewConversationMessage(conversationId, userName, message, true);
    }

    public async Task CompleteConversation(string conversationId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return;

        await BroadcastConversationCompleted(conversationId, userId);
    }
    
    // Multi-agent popup methods for escalation acceptance
    public async Task NotifyEscalationAccepted(string conversationId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return;
        
        // Dismiss popup for all other agents
        await Clients.GroupExcept("Agents", Context.ConnectionId).DismissEscalationPopup(conversationId);
    }

    // Real-time conversation updates
    public async Task JoinConversationGroup(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Conversation_{conversationId}");
        
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.Identity?.Name ?? "Agent";
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Clients.Group($"Conversation_{conversationId}").AgentJoinedConversation(conversationId, userId, userName);
        }
    }

    public async Task LeaveConversationGroup(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Conversation_{conversationId}");
        
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Clients.Group($"Conversation_{conversationId}").AgentLeftConversation(conversationId, userId);
        }
    }

    public async Task BroadcastNewMessageToAgents(string conversationId, object messageDto)
    {
        await Clients.Group($"Conversation_{conversationId}").NewMessageReceived(messageDto);

        // Also broadcast legacy/global event for components not in the conversation group (e.g., NotificationIndicator)
        try
        {
            string from = "User";
            string content = string.Empty;
            bool isFromAgent = false;

            // Attempt to extract fields dynamically
            if (messageDto is not null)
            {
                var type = messageDto.GetType();
                var roleProp = type.GetProperty("Role");
                var contentProp = type.GetProperty("Content");
                var userNameProp = type.GetProperty("UserName");
                var userIdProp = type.GetProperty("UserId");

                var role = roleProp?.GetValue(messageDto) as string;
                content = contentProp?.GetValue(messageDto) as string ?? string.Empty;
                from = (userNameProp?.GetValue(messageDto) as string)
                       ?? (userIdProp?.GetValue(messageDto) as string)
                       ?? (string.Equals(role, "agent", StringComparison.OrdinalIgnoreCase) ? "Agent" : "User");
                isFromAgent = string.Equals(role, "agent", StringComparison.OrdinalIgnoreCase);
            }

            await Clients.All.NewConversationMessage(conversationId, from, content, isFromAgent);
        }
        catch (Exception ex)
        {
            // Swallow: notification fan-out shouldn't break primary group delivery
            Console.WriteLine($"[ServerHub] Failed to broadcast legacy NewConversationMessage: {ex.Message}");
        }
    }

    public async Task BroadcastConversationStatusChanged(string conversationId, ConversationStatus status)
    {
        await Clients.Group($"Conversation_{conversationId}").ConversationStatusChanged(conversationId, status);
    }

    public async Task BroadcastCustomerTyping(string conversationId, bool isTyping)
    {
        await Clients.Group($"Conversation_{conversationId}").CustomerTyping(conversationId, isTyping);
    }

    public async Task BroadcastConversationViewersUpdated(string conversationId, object viewers)
    {
        await Clients.Group($"Conversation_{conversationId}").ConversationViewersUpdated(conversationId, viewers);
    }
}