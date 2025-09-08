// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.BotBuilderSamples
{
    public class ToolCall {
        public string Id { get; set; }
    }
    public class ConversationTurn
    {
        public string Role { get; set; } = null;
        public string Message { get; set; } = null;
        public string ImageData { get; set; } = null;
        public string ImageType { get; set; } = null;
        public string ToolCallId { get; set; } = null;
        public List<ChatToolCall> ToolCalls { get; set; } = null;
    }
    public class Attachment
    {
        public string Name { get; set; }
        public string ContentType { get; set; }
        public string Url { get; set; }
    }
    // Defines a state property used to track conversation data.
    public class ConversationData
    {
        // The ID of the user's thread.
        public string ThreadId { get; set; }
        public int MaxTurns { get; set; } = 10;
        
        // Conversation timeout settings
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public int TimeoutMinutes { get; set; } = 30;
        
        // Issue intake state tracking
        public bool IsIssueIntakeActive { get; set; } = false;
        public DateTime IssueIntakeStartedAt { get; set; }
        
        // Escalation state tracking
        public bool IsEscalated { get; set; } = false;
        public DateTime? EscalatedAt { get; set; }
        public string EscalationReason { get; set; }
        public string AssignedAgentId { get; set; }
        public bool IsHandoffPending { get; set; } = false;
        public DateTime? HandoffInitiatedAt { get; set; }
        public int? ConversationEntityId { get; set; } // Link to Conversation domain entity
        
        // Bot as Proxy agent handoff tracking
        public string AgentName { get; set; }
        public string CurrentAgentId { get; set; }
        public DateTime? HandoffStartedAt { get; set; }
        public DateTime? HandoffCompletedAt { get; set; }
        
        // Enhanced agent session tracking
        public bool IsAgentActivelyHandling { get; set; } = false;
        public DateTime? LastAgentActivity { get; set; }
        public int AgentSessionTimeoutMinutes { get; set; } = 30; // Configurable timeout
        public bool BotResponsesSuppressed { get; set; } = false;

        // Agent handback tracking
        public bool AgentRequestedHandback { get; set; } = false;
        public DateTime? HandbackRequestedAt { get; set; }
        public string HandbackReason { get; set; }
        public bool IsHandbackGracePeriod { get; set; } = false;
        public int HandbackGracePeriodMinutes { get; set; } = 2; // Time before bot resumes
        
        // Track conversation history
        public List<ConversationTurn> History = new List<ConversationTurn>();

        // Track attached documents
        public List<Attachment> Attachments = new List<Attachment>();

        public void AddTurn(string role, string message = "", string imageType = null, string imageData = null, string toolCallId = null, List<ChatToolCall> toolCalls = null)
        {
            if (imageType == null)
            {
                History.Add(new ConversationTurn { Role = role, Message = message, ToolCallId = toolCallId, ToolCalls = toolCalls });
            }
            else
            {
                History.Add(new ConversationTurn { Role = role, ImageType = imageType, ImageData = imageData });
            }
            
            // Update last activity timestamp
            LastActivity = DateTime.UtcNow;
            
            if (History.Count >= MaxTurns)
            {
                History.RemoveAt(1);
            }
        }
        
        public bool IsTimedOut()
        {
            return DateTime.UtcNow.Subtract(LastActivity).TotalMinutes > TimeoutMinutes;
        }
        
        public void StartIssueIntake()
        {
            IsIssueIntakeActive = true;
            IssueIntakeStartedAt = DateTime.UtcNow;
            LastActivity = DateTime.UtcNow;
        }
        
        public void CompleteIssueIntake()
        {
            IsIssueIntakeActive = false;
            CleanupIssueIntakeState();
        }
        
        public void CleanupIssueIntakeState()
        {
            // Remove issue collection state from history
            var stateMessages = History.Where(h => 
                h.Role == "system" && 
                (h.Message.StartsWith("ISSUE_STATE:") || 
                 h.Message == "INTAKE_INITIATED")).ToList();
                 
            foreach (var message in stateMessages)
            {
                History.Remove(message);
            }
        }
        
        public TimeSpan GetIssueIntakeDuration()
        {
            if (!IsIssueIntakeActive) return TimeSpan.Zero;
            return DateTime.UtcNow.Subtract(IssueIntakeStartedAt);
        }
        
        public void InitiateEscalation(string reason)
        {
            IsEscalated = true;
            IsHandoffPending = true;
            EscalatedAt = DateTime.UtcNow;
            EscalationReason = reason;
            HandoffInitiatedAt = DateTime.UtcNow;
            LastActivity = DateTime.UtcNow;
        }
        
        public void CompleteHandoffToAgent(string agentId)
        {
            AssignedAgentId = agentId;
            IsHandoffPending = false;
            LastActivity = DateTime.UtcNow;
        }
        
        public void HandbackToBot()
        {
            IsEscalated = false;
            AssignedAgentId = null;
            IsHandoffPending = false;
            LastActivity = DateTime.UtcNow;
        }
        
        public TimeSpan? GetEscalationDuration()
        {
            if (!IsEscalated || !EscalatedAt.HasValue) return null;
            return DateTime.UtcNow.Subtract(EscalatedAt.Value);
        }
        
        public string GetConversationTranscript()
        {
            return System.Text.Json.JsonSerializer.Serialize(History);
        }
        
        public string GetTimeoutWarningMessage()
        {
            var remainingMinutes = TimeoutMinutes - (int)DateTime.UtcNow.Subtract(LastActivity).TotalMinutes;
            
            if (remainingMinutes <= 5 && remainingMinutes > 0)
            {
                return $"⏰ This conversation will timeout in {remainingMinutes} minute(s). Please respond to continue.";
            }
            
            if (remainingMinutes <= 0)
            {
                return "⏰ This conversation has timed out. Please start a new conversation to report an issue.";
            }
            
            return string.Empty;
        }

        public List<ChatMessage> ToMessages()
        {
            var messages = History.Select<ConversationTurn, ChatMessage>((turn, index) =>
                turn.Role == "assistant" ? (turn.ToolCalls != null ? new AssistantChatMessage(toolCalls: turn.ToolCalls) : new AssistantChatMessage(turn.Message)) :
                turn.Role == "user" ? new UserChatMessage(new ChatMessageContentPart[]{
                    turn.ImageType == null ? 
                        ChatMessageContentPart.CreateTextPart(turn.Message) :
                        ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(Convert.FromBase64String(turn.ImageData)), turn.ImageType)
                }) :
                turn.Role == "system" ? new SystemChatMessage(turn.Message) :
                new ToolChatMessage(turn.ToolCallId, turn.Message)).ToList();
            return messages;
        }

        // Agent session management helper methods
        public bool ShouldBotRespond()
        {
            // Bot should not respond if agent is actively handling or during grace period
            if (IsAgentActivelyHandling || BotResponsesSuppressed || IsHandbackGracePeriod)
            {
                return false;
            }

            // Check for agent session timeout
            if (LastAgentActivity.HasValue)
            {
                var timeSinceLastActivity = DateTime.UtcNow - LastAgentActivity.Value;
                if (timeSinceLastActivity.TotalMinutes > AgentSessionTimeoutMinutes)
                {
                    // Auto-timeout the agent session
                    EndAgentSession("Agent session timeout");
                    return true;
                }
                return false; // Agent still active within timeout
            }

            return true; // No active agent session
        }

        public void StartAgentSession(string agentId, string agentName = null)
        {
            IsAgentActivelyHandling = true;
            CurrentAgentId = agentId;
            AgentName = agentName ?? agentId;
            LastAgentActivity = DateTime.UtcNow;
            BotResponsesSuppressed = true;
            
            // Clear any previous handback state
            AgentRequestedHandback = false;
            HandbackRequestedAt = null;
            HandbackReason = null;
            IsHandbackGracePeriod = false;
        }

        public void UpdateAgentActivity()
        {
            if (IsAgentActivelyHandling)
            {
                LastAgentActivity = DateTime.UtcNow;
            }
        }

        public void RequestHandback(string reason = null)
        {
            if (IsAgentActivelyHandling)
            {
                AgentRequestedHandback = true;
                HandbackRequestedAt = DateTime.UtcNow;
                HandbackReason = reason;
                IsHandbackGracePeriod = true;
            }
        }

        public void EndAgentSession(string reason = null)
        {
            IsAgentActivelyHandling = false;
            BotResponsesSuppressed = false;
            HandoffCompletedAt = DateTime.UtcNow;
            
            // Keep agent info for reference but clear active state
            LastAgentActivity = DateTime.UtcNow;
            
            // Clear handback state
            AgentRequestedHandback = false;
            IsHandbackGracePeriod = false;
        }

        public bool IsAgentSessionTimedOut()
        {
            if (!IsAgentActivelyHandling || !LastAgentActivity.HasValue)
                return false;

            var timeSinceLastActivity = DateTime.UtcNow - LastAgentActivity.Value;
            return timeSinceLastActivity.TotalMinutes > AgentSessionTimeoutMinutes;
        }

        public bool IsInHandbackGracePeriod()
        {
            if (!IsHandbackGracePeriod || !HandbackRequestedAt.HasValue)
                return false;

            var timeSinceHandbackRequest = DateTime.UtcNow - HandbackRequestedAt.Value;
            if (timeSinceHandbackRequest.TotalMinutes > HandbackGracePeriodMinutes)
            {
                // Grace period expired, end it
                IsHandbackGracePeriod = false;
                return false;
            }

            return true;
        }

    }
}
