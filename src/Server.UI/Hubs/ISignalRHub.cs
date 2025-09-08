using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;
using CleanArchitecture.Blazor.Application.Features.Issues.DTOs;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Server.UI.Hubs;

public interface ISignalRHub
{
    public const string Url = "/signalRHub";

    Task Connect(string connectionId, string userName);
    Task Disconnect(string connectionId, string userName);

    Task Start(int id, string message);
    Task Completed(int id, string message);

    Task SendMessage(string from, string message);
    Task SendPrivateMessage(string from, string to, string message);
    Task SendNotification(string message);

    // Active page-component session signaling
    Task PageComponentOpened(string pageComponent, string userId, string userName);
    Task PageComponentClosed(string pageComponent, string userId, string userName);

    // Snapshot method: fetch current online users with profile data
    // Note: invoked via HubConnection.InvokeAsync from clients
    Task<List<UserContext>> GetOnlineUsers();

    // Issue real-time update methods
    Task IssueCreated(IssueListDto issue);
    Task IssueUpdated(IssueListDto issue);
    Task IssueStatusChanged(Guid issueId, IssueStatus newStatus);
    Task IssueListUpdated();
    
    // Agent escalation methods
    Task ConversationEscalated(string conversationId, string reason, string customerPhoneNumber);
    Task ConversationAssigned(string conversationId, string agentId, string agentName);
    Task ConversationCompleted(string conversationId, string agentId);
    Task AgentStatusChanged(string agentId, string status);
    Task NewConversationMessage(string conversationId, string from, string message, bool isFromAgent);
    
    // Persistent escalation notification methods
    Task EscalationPersistentNotification(string conversationId, string reason, string customerPhoneNumber, int priority, DateTime escalatedAt);
    Task EscalationAccepted(string conversationId);
    Task EscalationIgnored(string conversationId, string agentId);
    
    // Enhanced agent notification methods
    Task EscalationNotification(string conversationId, string reason, string customerPhoneNumber, int priority, DateTime escalatedAt);
    Task TargetedEscalationNotification(string conversationId, string reason, string customerPhoneNumber, int priority, DateTime escalatedAt);
    Task AgentWorkloadUpdated(int agentId, int activeCount, int maxCount, bool isAvailable);
    Task EscalationQueueUpdated(int queueCount);
    Task AgentAvailabilityChanged(int agentId, string status, bool isAvailable, string agentName);
    
    // Multi-agent popup methods
    Task ReceiveEscalationPopup(object escalationPopupDto);
    Task DismissEscalationPopup(string conversationId);
    Task NotifyEscalationAccepted(string conversationId);
    
    // Real-time conversation updates
    Task NewMessageReceived(object messageDto);
    Task ConversationStatusChanged(string conversationId, ConversationStatus status);
    Task CustomerTyping(string conversationId, bool isTyping);
    Task AgentJoinedConversation(string conversationId, string agentId, string agentName);
    Task AgentLeftConversation(string conversationId, string agentId);
    Task ConversationViewersUpdated(string conversationId, object viewers);
    
    // Issue-Conversation Integration methods
    Task AgentJoinedIssueConversation(Guid issueId, string agentId, string agentName);
    Task AgentLeftIssueConversation(Guid issueId, string agentId, string agentName);
    Task IssueConversationMessageReceived(Guid issueId, object message);
}