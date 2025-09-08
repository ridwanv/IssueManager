# Agent Session Management with Handback - Implementation Plan

## Overview
This plan ensures that when an agent is actively handling a conversation, the bot will gracefully step back and only resume when appropriate, providing a seamless handover experience for users. It also includes comprehensive handback functionality allowing agents to return conversations to the bot.

## Current State Analysis

### Current Bot Response Flow:
1. Message comes in → `OnMessageActivityAsync()`
2. Bot checks for special activity types (agentMessage, handoffStatus, etc.)
3. Bot processes regular user messages through AI
4. Bot sends responses regardless of escalation state

### Problem:
- Bot continues to respond to user messages even when `conversationData.IsEscalated = true`
- This creates confusion with dual responses (bot + agent)
- No mechanism to suppress bot responses during active agent sessions

## Implementation Plan

### Phase 1: Enhanced ConversationData State Tracking

**Add New Properties to ConversationData:**
```csharp
// Enhanced agent session tracking
public bool IsAgentActivelyHandling { get; set; } = false;
public DateTime? LastAgentActivity { get; set; }
public int AgentSessionTimeoutMinutes { get; set; } = 30; // Configurable timeout
public bool BotResponsesSuppressed { get; set; } = false;

// Agent handback tracking
public bool AgentRequestedHandback { get; set; } = false;
public DateTime? HandbackRequestedAt { get; set; }
public string? HandbackReason { get; set; }
public bool IsHandbackGracePeriod { get; set; } = false;
public int HandbackGracePeriodMinutes { get; set; } = 2; // Time before bot resumes
```

**Add State Management Methods:**
```csharp
public void StartAgentSession(string agentId, string agentName)
public void EndAgentSession(string? reason = null)
public void RequestAgentHandback(string reason)
public void CompleteHandback()
public bool IsAgentSessionActive()
public bool IsAgentSessionTimedOut()
public bool IsInHandbackGracePeriod()
public void UpdateAgentActivity()
```

### Phase 2: Update Message Processing Logic in OnMessageActivityAsync()

**Enhanced Flow with Early Exit Checks:**
```csharp
protected override async Task<bool> OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
{
    // Load conversation state first
    var conversationData = await LoadConversationData(turnContext);

    // Handle special activity types (agent messages, handoff status, etc.)
    // ... existing special handling code ...

    // EARLY EXIT: Check if agent is actively handling conversation
    if (conversationData.IsAgentActivelyHandling)
    {
        // Check for agent session timeout
        if (conversationData.IsAgentSessionTimedOut())
        {
            await HandleAgentSessionTimeout(turnContext, conversationData, cancellationToken);
            // Continue to bot processing after timeout
        }
        else
        {
            // Agent is active - suppress bot responses
            await StoreUserMessageOnly(turnContext, conversationData, cancellationToken);
            return true; // Exit without bot response
        }
    }

    // GRACE PERIOD: Check if we're in handback grace period
    if (conversationData.IsInHandbackGracePeriod())
    {
        await HandleHandbackGracePeriod(turnContext, conversationData, cancellationToken);
        return true; // Exit during grace period
    }

    // Normal bot processing continues...
    // ... existing AI processing logic ...
}
```

### Phase 3: Agent Session Lifecycle Management

**Session Start Triggers:**
- Agent sends first message via `HandleAgentMessageActivity()`
- Escalation accepted/assigned event
- Manual agent takeover via new API endpoint

**Session End Triggers:**
- **Agent explicit handback**: New "handback" command/button
- Agent inactivity timeout (30 minutes default)
- Conversation completion
- **Agent handback message**: Special message type like "/handback" or via UI

**Session Maintenance:**
- Update `LastAgentActivity` on every agent message
- Monitor for agent presence/typing indicators
- Handle agent disconnections gracefully

### Phase 4: Agent Handback Implementation

**New Handback Mechanisms:**

1. **UI Handback Button** (in agent dashboard):
```csharp
[HttpPost("api/conversations/{conversationId}/handback")]
public async Task<IActionResult> HandbackToBot(string conversationId, [FromBody] HandbackRequest request)
```

2. **Agent Message Commands**:
   - Agent types `/handback` or `/end` 
   - Special handback activity type
   - Graceful transition message to user

3. **Automatic Handback**:
   - After issue resolution
   - On conversation completion
   - On agent timeout

**Handback Process Flow:**
```csharp
private async Task ProcessAgentHandback(ITurnContext turnContext, ConversationData conversationData, string reason, CancellationToken cancellationToken)
{
    // 1. Mark handback requested
    conversationData.RequestAgentHandback(reason);
    
    // 2. Send handback notification to user
    var handbackMessage = $"Agent {conversationData.AgentName} has completed assistance. I'm here to help if you need anything else!";
    await turnContext.SendActivityAsync(MessageFactory.Text(handbackMessage), cancellationToken);
    
    // 3. Start grace period (optional - immediate or delayed bot resumption)
    conversationData.IsHandbackGracePeriod = true;
    conversationData.HandbackRequestedAt = DateTime.UtcNow;
    
    // 4. After grace period, resume bot responses
    // (handled in message processing logic)
}
```

### Phase 5: Enhanced Bot Behavior During Agent Sessions

**Suppressed Responses During Active Agent Session:**
- No AI responses to user messages
- No escalation suggestions
- No automated issue intake
- Store user messages silently for agent context

**Allowed Responses During Agent Session:**
- System messages (escalation status, handoff notifications)
- Agent presence indicators (typing, joining, leaving)
- Emergency/safety responses if configured
- Agent handback confirmations

**Resumed Responses After Handback:**
- Welcome back message: "I'm back to assist you! How can I help?"
- Full AI response capabilities restored
- Access to full conversation history including agent messages

### Phase 6: User Experience Enhancements

**Status Indicators:**
- **Agent active**: "You're now chatting with [Agent Name]"
- **Agent typing**: Forward typing indicators
- **Agent handback**: "Agent has completed assistance. I'm here to help!"
- **Bot resumed**: "I'm back to assist you! How can I help?"

**Handback Communication:**
```csharp
private async Task NotifyUserOfHandback(ITurnContext turnContext, ConversationData conversationData, CancellationToken cancellationToken)
{
    var messages = new[]
    {
        $"✅ {conversationData.AgentName} has completed their assistance.",
        "I'm here to help if you need anything else!",
        "Feel free to ask me any questions or start a new request."
    };
    
    foreach (var message in messages)
    {
        await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
        await Task.Delay(1000, cancellationToken); // Brief pause between messages
    }
}
```

### Phase 7: API Endpoints for Agent Handback

**New Controller Actions:**
```csharp
[HttpPost("api/conversations/{conversationId}/handback")]
public async Task<IActionResult> RequestHandback(string conversationId, [FromBody] HandbackRequest request)

[HttpPost("api/conversations/{conversationId}/resume-bot")]
public async Task<IActionResult> ResumeBotResponse(string conversationId)

[HttpGet("api/conversations/{conversationId}/agent-status")]
public async Task<IActionResult> GetAgentStatus(string conversationId)
```

**Handback Request Model:**
```csharp
public class HandbackRequest
{
    public string Reason { get; set; } = "Assistance completed";
    public bool ImmediateHandback { get; set; } = false; // Skip grace period
    public string? HandbackMessage { get; set; } // Custom message to user
}
```

### Phase 8: Integration with Existing HandleAgentMessageActivity

**Enhanced Agent Message Handler:**
```csharp
private async Task<bool> HandleAgentMessageActivity(ITurnContext<IMessageActivity> turnContext, ConversationData conversationData, CancellationToken cancellationToken)
{
    // ... existing code ...

    // Check for handback commands in agent message
    if (IsHandbackCommand(agentMessage))
    {
        await ProcessAgentHandback(turnContext, conversationData, "Agent requested handback", cancellationToken);
        return true;
    }

    // Update agent session activity
    conversationData.UpdateAgentActivity();
    
    // Start agent session if not already active
    if (!conversationData.IsAgentActivelyHandling)
    {
        conversationData.StartAgentSession(agentId, agentName);
        await NotifyUserOfAgentJoin(turnContext, agentName, cancellationToken);
    }

    // ... rest of existing code ...
}

private bool IsHandbackCommand(string message)
{
    var handbackCommands = new[] { "/handback", "/end", "/complete", "handback to bot" };
    return handbackCommands.Any(cmd => message.ToLowerInvariant().Contains(cmd));
}
```

### Phase 9: Configuration & Monitoring

**Enhanced Configuration:**
```json
{
  "AgentSession": {
    "TimeoutMinutes": 30,
    "HandbackGracePeriodMinutes": 2,
    "EnableAutomaticHandback": true,
    "EnableHandbackCommands": true,
    "ShowAgentStatusToUser": true,
    "HandbackCommands": ["/handback", "/end", "/complete"]
  }
}
```

**Logging & Metrics:**
- Agent session start/end events
- Handback trigger reasons (timeout, manual, command)
- Session duration tracking
- User satisfaction during transitions
- Bot response suppression metrics

## Key Considerations

### Edge Cases to Handle:
1. **Multiple Agents**: What if multiple agents try to handle same conversation?
2. **Agent Disconnection**: How to detect and handle unexpected disconnections?
3. **Emergency Escalation**: Should bot ever override agent session for urgent issues?
4. **Partial Messages**: What if agent is typing when user sends message?

### Performance Impact:
- Minimal state storage overhead
- No impact on non-escalated conversations
- Efficient timeout checking

### Backwards Compatibility:
- Existing escalation flows continue to work
- Gradual migration to new session management
- Fallback to current behavior if session data missing

## Key Benefits

1. **Seamless Handback**: Agents can gracefully hand conversations back to the bot
2. **User Clarity**: Clear communication about who is handling the conversation
3. **Flexible Control**: Multiple handback mechanisms (UI, commands, automatic)
4. **Conversation Continuity**: Bot has full context when resuming
5. **Configurable Behavior**: Timeouts, grace periods, and handback methods configurable
6. **Robust State Management**: Proper tracking of agent sessions and transitions

## Summary

This plan ensures that agents have full control over when to hand conversations back to the bot, while maintaining a smooth user experience and preventing dual responses from both agent and bot. The implementation provides multiple handback mechanisms, proper session state management, and clear communication to users about conversation handoffs.