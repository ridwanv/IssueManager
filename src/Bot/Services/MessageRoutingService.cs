using Microsoft.Bot.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.BotBuilderSamples;

namespace IssueManager.Bot.Services
{
    /// <summary>
    /// Service for routing messages based on conversation context and user state
    /// </summary>
    public class MessageRoutingService
    {
        private readonly ILogger<MessageRoutingService> _logger;
        private readonly UserState _userState;
        private readonly ConversationState _conversationState;

        public MessageRoutingService(
            ILogger<MessageRoutingService> logger, 
            UserState userState, 
            ConversationState conversationState)
        {
            _logger = logger;
            _userState = userState;
            _conversationState = conversationState;
        }

        /// <summary>
        /// Determines if this is a new conversation or ongoing based on user state
        /// </summary>
        /// <param name="turnContext">Bot turn context</param>
        /// <returns>True if new conversation, false if ongoing</returns>
        public async Task<bool> IsNewConversationAsync(ITurnContext turnContext)
        {
            try
            {
                var userProfileAccessor = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
                var userProfile = await userProfileAccessor.GetAsync(turnContext, () => new UserProfile());

                // Check if user has interacted before
                if (string.IsNullOrEmpty(userProfile.PhoneNumber))
                {
                    _logger.LogInformation("New user detected - no previous profile found");
                    return true;
                }

                // Check if conversation has been inactive for more than 24 hours
                var lastActivity = userProfile.LastActivity;
                if (lastActivity.HasValue && (DateTime.UtcNow - lastActivity.Value).TotalHours > 24)
                {
                    _logger.LogInformation("Conversation expired for user {PhoneNumber} - treating as new", userProfile.PhoneNumber);
                    return true;
                }

                _logger.LogDebug("Existing conversation found for user {PhoneNumber}", userProfile.PhoneNumber);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining conversation state");
                // Default to new conversation to be safe
                return true;
            }
        }

        /// <summary>
        /// Updates user profile with latest activity
        /// </summary>
        /// <param name="turnContext">Bot turn context</param>
        /// <param name="phoneNumber">User's phone number</param>
        /// <returns>Updated user profile</returns>
        public async Task<UserProfile> UpdateUserActivityAsync(ITurnContext turnContext, string phoneNumber)
        {
            try
            {
                var userProfileAccessor = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
                var userProfile = await userProfileAccessor.GetAsync(turnContext, () => new UserProfile());

                userProfile.PhoneNumber = phoneNumber;
                userProfile.LastActivity = DateTime.UtcNow;
                userProfile.MessageCount++;

                await userProfileAccessor.SetAsync(turnContext, userProfile);
                await _userState.SaveChangesAsync(turnContext);

                _logger.LogDebug("Updated activity for user {PhoneNumber}, message count: {Count}", 
                    phoneNumber, userProfile.MessageCount);

                return userProfile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user activity for {PhoneNumber}", phoneNumber);
                return new UserProfile { PhoneNumber = phoneNumber, LastActivity = DateTime.UtcNow };
            }
        }

        /// <summary>
        /// Gets current conversation context
        /// </summary>
        /// <param name="turnContext">Bot turn context</param>
        /// <returns>Conversation data</returns>
        public async Task<ConversationData> GetConversationContextAsync(ITurnContext turnContext)
        {
            try
            {
                var conversationDataAccessor = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
                var conversationData = await conversationDataAccessor.GetAsync(turnContext, () => new ConversationData());

                return conversationData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation context");
                return new ConversationData();
            }
        }

        /// <summary>
        /// Updates conversation context with new information
        /// </summary>
        /// <param name="turnContext">Bot turn context</param>
        /// <param name="conversationData">Updated conversation data</param>
        public async Task UpdateConversationContextAsync(ITurnContext turnContext, ConversationData conversationData)
        {
            try
            {
                var conversationDataAccessor = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
                await conversationDataAccessor.SetAsync(turnContext, conversationData);
                await _conversationState.SaveChangesAsync(turnContext);

                _logger.LogDebug("Updated conversation context for conversation {ConversationId}", 
                    turnContext.Activity.Conversation.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating conversation context");
            }
        }

        /// <summary>
        /// Routes message based on conversation state and content
        /// </summary>
        /// <param name="turnContext">Bot turn context</param>
        /// <param name="messageData">Parsed message data</param>
        /// <returns>Routing decision</returns>
        public async Task<MessageRoutingDecision> RouteMessageAsync(ITurnContext turnContext, WhatsAppMessageData messageData)
        {
            try
            {
                var isNewConversation = await IsNewConversationAsync(turnContext);
                var userProfile = await UpdateUserActivityAsync(turnContext, messageData.From);
                var conversationContext = await GetConversationContextAsync(turnContext);

                var routingDecision = new MessageRoutingDecision
                {
                    IsNewConversation = isNewConversation,
                    UserProfile = userProfile,
                    ConversationContext = conversationContext,
                    MessageType = DetermineMessageIntent(messageData.Text),
                    RequiresGreeting = isNewConversation,
                    RequiresTypingIndicator = true,
                    RequiresReadReceipt = true
                };

                _logger.LogInformation("Message routed for user {PhoneNumber}: New={IsNew}, Intent={Intent}", 
                    messageData.From, isNewConversation, routingDecision.MessageType);

                return routingDecision;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error routing message from {From}", messageData.From);
                return new MessageRoutingDecision
                {
                    IsNewConversation = true,
                    MessageType = MessageIntent.GeneralInquiry,
                    RequiresGreeting = true,
                    RequiresTypingIndicator = true,
                    RequiresReadReceipt = true
                };
            }
        }

        private MessageIntent DetermineMessageIntent(string? messageText)
        {
            if (string.IsNullOrEmpty(messageText))
                return MessageIntent.GeneralInquiry;

            var text = messageText.ToLowerInvariant();

            // Simple keyword-based intent detection
            var urgentKeywords = new[] { "urgent", "critical", "emergency", "asap", "immediately" };
            var issueKeywords = new[] { "problem", "issue", "error", "bug", "broken", "not working", "failed" };
            var greetingKeywords = new[] { "hello", "hi", "hey", "good morning", "good afternoon" };

            if (greetingKeywords.Any(keyword => text.Contains(keyword)))
                return MessageIntent.Greeting;

            if (urgentKeywords.Any(keyword => text.Contains(keyword)))
                return MessageIntent.UrgentIssue;

            if (issueKeywords.Any(keyword => text.Contains(keyword)))
                return MessageIntent.IssueReport;

            return MessageIntent.GeneralInquiry;
        }
    }

    /// <summary>
    /// Message routing decision result
    /// </summary>
    public class MessageRoutingDecision
    {
        public bool IsNewConversation { get; set; }
        public UserProfile UserProfile { get; set; } = new();
        public ConversationData ConversationContext { get; set; } = new();
        public MessageIntent MessageType { get; set; }
        public bool RequiresGreeting { get; set; }
        public bool RequiresTypingIndicator { get; set; }
        public bool RequiresReadReceipt { get; set; }
    }

    /// <summary>
    /// Message intent classification
    /// </summary>
    public enum MessageIntent
    {
        Greeting,
        IssueReport,
        UrgentIssue,
        GeneralInquiry,
        FollowUp
    }
}