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

        public List<ChatMessage> toMessages()
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

    }
}
