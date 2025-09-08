// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Text.Json;
using Microsoft.BotBuilderSamples;

namespace IssueManager.Bot.Services;

public class ConversationHandoffService
{
    public async Task<Activity> CreateHandoffInitiation(
        ITurnContext turnContext, 
        ConversationData conversationData,
        string reason,
        string? contextSummary = null)
    {
        var handoffInitiation = Activity.CreateEventActivity();
        handoffInitiation.Name = "handoff.initiate";
        handoffInitiation.From = turnContext.Activity.Recipient;
        handoffInitiation.Recipient = turnContext.Activity.From;
        handoffInitiation.Conversation = turnContext.Activity.Conversation;
        handoffInitiation.ChannelId = turnContext.Activity.ChannelId;
        
        // Create handoff context
        var handoffContext = new
        {
            Skill = "customer_support",
            Reason = reason,
            ConversationId = turnContext.Activity.Conversation.Id,
            UserId = turnContext.Activity.From.Id,
            UserName = turnContext.Activity.From.Name,
            EscalatedAt = DateTime.UtcNow,
            Summary = contextSummary ?? "User requested human assistance",
            ConversationHistory = conversationData.GetConversationTranscript(),
            IsIssueIntakeActive = conversationData.IsIssueIntakeActive,
            ConversationDuration = conversationData.GetIssueIntakeDuration().TotalMinutes,
            MessageCount = conversationData.History.Count
        };
        
        handoffInitiation.Value = handoffContext;
        
        // Add transcript as attachment if conversation history exists
        if (conversationData.History.Any())
        {
            var transcript = new Microsoft.Bot.Schema.Attachment
            {
                ContentType = "application/json",
                Name = "conversation_transcript.json",
                Content = conversationData.History
            };
            
            ((Activity)handoffInitiation).Attachments = new[] { transcript };
        }
        
        return (Activity)handoffInitiation;
    }
    
    public Activity CreateHandoffStatus(
        ITurnContext turnContext,
        string state, // "accepted", "failed", "completed"
        string? message = null,
        string? agentId = null,
        string? agentName = null)
    {
        var handoffStatus = Activity.CreateEventActivity();
        handoffStatus.Name = "handoff.status";
        handoffStatus.From = turnContext.Activity.Recipient;
        handoffStatus.Recipient = turnContext.Activity.From;
        handoffStatus.Conversation = turnContext.Activity.Conversation;
        handoffStatus.ChannelId = turnContext.Activity.ChannelId;
        
        var statusContext = new
        {
            State = state,
            Message = message ?? GetDefaultStatusMessage(state),
            AgentId = agentId,
            AgentName = agentName,
            Timestamp = DateTime.UtcNow,
            ConversationId = turnContext.Activity.Conversation.Id
        };
        
        handoffStatus.Value = statusContext;
        return (Activity)handoffStatus;
    }
    
    public bool IsHandoffEvent(Activity activity)
    {
        return activity.Type == ActivityTypes.Event && 
               (activity.Name == "handoff.initiate" || activity.Name == "handoff.status");
    }
    
    public async Task<bool> HandleHandoffEvent(ITurnContext turnContext, ConversationData conversationData)
    {
        if (!IsHandoffEvent(turnContext.Activity))
            return false;
            
        switch (turnContext.Activity.Name)
        {
            case "handoff.initiate":
                return await ProcessHandoffInitiation(turnContext, conversationData);
                
            case "handoff.status":
                return await ProcessHandoffStatus(turnContext, conversationData);
                
            default:
                return false;
        }
    }
    
    private async Task<bool> ProcessHandoffInitiation(ITurnContext turnContext, ConversationData conversationData)
    {
        try
        {
            // Mark conversation as escalated if not already
            if (!conversationData.IsEscalated)
            {
                conversationData.InitiateEscalation("Handoff event received");
            }
            
            // Notify user about handoff initiation
            var message = "🔄 Your conversation is being transferred to a human agent. Please hold on...";
            await turnContext.SendActivityAsync(MessageFactory.Text(message));
            
            return true;
        }
        catch (Exception)
        {
            // Log error but don't throw - let conversation continue
            return false;
        }
    }
    
    private async Task<bool> ProcessHandoffStatus(ITurnContext turnContext, ConversationData conversationData)
    {
        try
        {
            var statusJson = JsonSerializer.Deserialize<JsonElement>(turnContext.Activity.Value.ToString() ?? "{}");
            var state = statusJson.GetProperty("state").GetString();
            var agentName = statusJson.TryGetProperty("agentName", out var agentNameProp) ? 
                           agentNameProp.GetString() : "Support Agent";
            
            string message = state switch
            {
                "accepted" => $"✅ {agentName} has joined the conversation and will assist you shortly.",
                "failed" => "❌ We're having trouble connecting you to an agent. Please try again or contact support directly.",
                "completed" => $"✅ Your conversation with {agentName} has been completed. Thank you for contacting support!",
                _ => $"Handoff status updated: {state}"
            };
            
            await turnContext.SendActivityAsync(MessageFactory.Text(message));
            
            // Update conversation data based on status
            switch (state)
            {
                case "accepted":
                    var agentId = statusJson.TryGetProperty("agentId", out var agentIdProp) ? 
                                 agentIdProp.GetString() : null;
                    if (!string.IsNullOrEmpty(agentId))
                    {
                        conversationData.CompleteHandoffToAgent(agentId);
                    }
                    break;
                    
                case "completed":
                    conversationData.HandbackToBot();
                    break;
                    
                case "failed":
                    // Reset escalation state to allow retry
                    conversationData.HandbackToBot();
                    break;
            }
            
            return true;
        }
        catch (Exception)
        {
            // Log error but don't throw
            return false;
        }
    }
    
    /// <summary>
    /// Creates an agent presence activity for typing indicators
    /// </summary>
    public Activity CreateAgentPresenceActivity(
        ITurnContext turnContext,
        string agentId,
        string agentName,
        bool isTyping)
    {
        var presenceActivity = Activity.CreateEventActivity();
        presenceActivity.Name = "agent.presence";
        presenceActivity.Type = "agentPresence";
        presenceActivity.From = new ChannelAccount { Id = agentId, Name = agentName };
        presenceActivity.Recipient = turnContext.Activity.From;
        presenceActivity.Conversation = turnContext.Activity.Conversation;
        presenceActivity.ChannelId = turnContext.Activity.ChannelId;
        
        presenceActivity.Value = new
        {
            MessageType = "AgentPresence",
            IsTyping = isTyping,
            AgentId = agentId,
            AgentName = agentName,
            Timestamp = DateTime.UtcNow
        };
        
        return (Activity)presenceActivity;
    }

    /// <summary>
    /// Updates conversation state for active agent handoff sessions
    /// </summary>
    public void UpdateHandoffSession(
        ConversationData conversationData,
        string agentId,
        string agentName,
        string status)
    {
        switch (status.ToLowerInvariant())
        {
            case "connected":
            case "active":
                conversationData.IsEscalated = true;
                conversationData.AgentName = agentName;
                conversationData.CurrentAgentId = agentId;
                conversationData.HandoffStartedAt = DateTime.UtcNow;
                break;
                
            case "completed":
            case "ended":
                conversationData.HandbackToBot();
                conversationData.CurrentAgentId = null;
                conversationData.HandoffCompletedAt = DateTime.UtcNow;
                break;
        }
    }

    /// <summary>
    /// Validates if the conversation is in a valid state for agent message routing
    /// </summary>
    public bool IsValidForAgentRouting(ConversationData conversationData, string agentId)
    {
        return conversationData.IsEscalated && 
               !string.IsNullOrEmpty(conversationData.CurrentAgentId) &&
               conversationData.CurrentAgentId == agentId;
    }

    /// <summary>
    /// Creates handoff status events for agent message delivery confirmation
    /// </summary>
    public Activity CreateMessageDeliveryConfirmation(
        ITurnContext turnContext,
        string agentId,
        string agentName,
        string messageId,
        bool delivered)
    {
        var confirmationActivity = Activity.CreateEventActivity();
        confirmationActivity.Name = "agent.message.delivery";
        confirmationActivity.From = new ChannelAccount { Id = agentId, Name = agentName };
        confirmationActivity.Recipient = turnContext.Activity.From;
        confirmationActivity.Conversation = turnContext.Activity.Conversation;
        confirmationActivity.ChannelId = turnContext.Activity.ChannelId;
        
        confirmationActivity.Value = new
        {
            MessageType = "DeliveryConfirmation",
            MessageId = messageId,
            Delivered = delivered,
            AgentId = agentId,
            AgentName = agentName,
            Timestamp = DateTime.UtcNow
        };
        
        return (Activity)confirmationActivity;
    }

    /// <summary>
    /// Processes agent message activities and maintains conversation context
    /// </summary>
    public async Task<bool> ProcessAgentMessageActivity(
        ITurnContext turnContext, 
        ConversationData conversationData)
    {
        try
        {
            if (turnContext.Activity.Type != "agentMessage")
                return false;

            var agentId = turnContext.Activity.From?.Id;
            var agentName = turnContext.Activity.From?.Name ?? "Agent";
            
            if (string.IsNullOrEmpty(agentId))
                return false;

            // Validate agent has permission to send messages in this conversation
            if (!IsValidForAgentRouting(conversationData, agentId))
            {
                Console.WriteLine($"Agent {agentId} not authorized for conversation routing");
                return false;
            }

            // Update conversation state to maintain handoff context
            UpdateHandoffSession(conversationData, agentId, agentName, "active");
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing agent message activity: {ex.Message}");
            return false;
        }
    }
    
    private string GetDefaultStatusMessage(string state)
    {
        return state switch
        {
            "accepted" => "Agent has accepted the handoff and will assist shortly.",
            "failed" => "Handoff to agent failed. Please try again.",
            "completed" => "Agent conversation completed.",
            _ => $"Handoff status: {state}"
        };
    }
}