using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WhatsApp.Api.Services
{
    /// <summary>
    /// Service for managing DirectLine conversations with Azure Bot Service
    /// </summary>
    public class DirectLineService
    {
        private readonly ILogger<DirectLineService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _directLineSecret;
        private readonly ConcurrentDictionary<string, string> _phoneToConversationMap;
        private readonly ConcurrentDictionary<string, DirectLineClient> _directLineClients;

        public DirectLineService(ILogger<DirectLineService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _directLineSecret = Environment.GetEnvironmentVariable("DirectLine__Secret") ?? 
                               _configuration.GetValue<string>("DirectLine:Secret") ?? 
                               throw new InvalidOperationException("DirectLine Secret is required");
            
            _phoneToConversationMap = new ConcurrentDictionary<string, string>();
            _directLineClients = new ConcurrentDictionary<string, DirectLineClient>();
        }

        /// <summary>
        /// Sends a message to the bot via DirectLine
        /// </summary>
        /// <param name="phoneNumber">WhatsApp phone number</param>
        /// <param name="messageText">Message text</param>
        /// <returns>Task</returns>
        public async Task SendMessageToBotAsync(string phoneNumber, string messageText)
        {
            try
            {
                var conversationId = await GetOrCreateConversationAsync(phoneNumber);
                var client = GetDirectLineClient();

                var activity = new Activity
                {
                    Type = ActivityTypes.Message,
                    From = new ChannelAccount { Id = phoneNumber, Name = phoneNumber },
                    Text = messageText,
                    ChannelId = "whatsapp"
                };

                var response = await client.Conversations.PostActivityAsync(conversationId, activity);
                
                if (string.IsNullOrEmpty(response?.Id))
                {
                    _logger.LogWarning("Failed to send message to DirectLine for {PhoneNumber}", phoneNumber);
                }
                else
                {
                    _logger.LogDebug("Message sent to DirectLine successfully for {PhoneNumber}, ActivityId: {ActivityId}", 
                        phoneNumber, response.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to DirectLine for {PhoneNumber}", phoneNumber);
                throw;
            }
        }

        /// <summary>
        /// Gets or creates a DirectLine conversation for a phone number
        /// </summary>
        /// <param name="phoneNumber">WhatsApp phone number</param>
        /// <returns>Conversation ID</returns>
        public async Task<string> GetOrCreateConversationAsync(string phoneNumber)
        {
            if (_phoneToConversationMap.TryGetValue(phoneNumber, out var existingConversationId))
            {
                return existingConversationId;
            }

            try
            {
                var client = GetDirectLineClient();
                var conversation = await client.Conversations.StartConversationAsync();

                if (conversation?.ConversationId == null)
                {
                    throw new InvalidOperationException("Failed to create DirectLine conversation");
                }

                _phoneToConversationMap.TryAdd(phoneNumber, conversation.ConversationId);
                
                _logger.LogInformation("Created new DirectLine conversation {ConversationId} for {PhoneNumber}", 
                    conversation.ConversationId, phoneNumber);

                return conversation.ConversationId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating DirectLine conversation for {PhoneNumber}", phoneNumber);
                throw;
            }
        }

        /// <summary>
        /// Gets the conversation ID for a phone number
        /// </summary>
        /// <param name="phoneNumber">WhatsApp phone number</param>
        /// <returns>Conversation ID or null if not found</returns>
        public string? GetConversationId(string phoneNumber)
        {
            return _phoneToConversationMap.TryGetValue(phoneNumber, out var conversationId) ? conversationId : null;
        }

        /// <summary>
        /// Gets activities from a DirectLine conversation
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="watermark">Watermark for retrieving new activities</param>
        /// <returns>Activities set</returns>
        public async Task<ActivitySet?> GetActivitiesAsync(string conversationId, string? watermark = null)
        {
            try
            {
                var client = GetDirectLineClient();
                return await client.Conversations.GetActivitiesAsync(conversationId, watermark);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activities from DirectLine conversation {ConversationId}", conversationId);
                return null;
            }
        }

        /// <summary>
        /// Gets or creates a DirectLine client
        /// </summary>
        /// <returns>DirectLine client</returns>
        private DirectLineClient GetDirectLineClient()
        {
            // Use a single client instance for simplicity
            // In production, you might want to implement client pooling
            return _directLineClients.GetOrAdd("default", _ => new DirectLineClient(_directLineSecret));
        }
    }
}