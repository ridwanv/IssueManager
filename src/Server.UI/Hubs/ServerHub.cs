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
        await base.OnConnectedAsync().ConfigureAwait(false);
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

    public async Task BroadcastConversationEscalated(int conversationId, string reason, string customerPhoneNumber)
    {
        await Clients.Group("Agents").ConversationEscalated(conversationId, reason, customerPhoneNumber).ConfigureAwait(false);
    }

    public async Task BroadcastConversationAssigned(int conversationId, string agentId, string agentName)
    {
        await Clients.All.ConversationAssigned(conversationId, agentId, agentName).ConfigureAwait(false);
    }

    public async Task BroadcastConversationCompleted(int conversationId, string agentId)
    {
        await Clients.All.ConversationCompleted(conversationId, agentId).ConfigureAwait(false);
    }

    public async Task BroadcastAgentStatusChanged(string agentId, string status)
    {
        await Clients.Group("Agents").AgentStatusChanged(agentId, status).ConfigureAwait(false);
    }

    public async Task BroadcastNewConversationMessage(int conversationId, string from, string message, bool isFromAgent)
    {
        await Clients.All.NewConversationMessage(conversationId, from, message, isFromAgent).ConfigureAwait(false);
    }

    // Agent-specific methods for conversation management
    public async Task AcceptConversation(int conversationId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.Identity?.Name ?? "Agent";
        
        if (string.IsNullOrEmpty(userId)) return;

        await BroadcastConversationAssigned(conversationId, userId, userName);
    }

    public async Task SendConversationMessage(int conversationId, string message)
    {
        var userName = Context.User?.Identity?.Name ?? "Agent";
        await BroadcastNewConversationMessage(conversationId, userName, message, true);
    }

    public async Task CompleteConversation(int conversationId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return;

        await BroadcastConversationCompleted(conversationId, userId);
    }
}