using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Bot.Connector.DirectLine;

namespace WhatsApp.Api.Services
{
    /// <summary>
    /// Background service that polls DirectLine for bot responses and sends them to WhatsApp
    /// </summary>
    public class WhatsAppDirectLineService : BackgroundService
    {
        private readonly ILogger<WhatsAppDirectLineService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<string, string> _conversationWatermarks;
        private readonly ConcurrentDictionary<string, DateTime> _lastActivityTime;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(2);
        private readonly TimeSpan _conversationTimeout = TimeSpan.FromMinutes(30);

        public WhatsAppDirectLineService(
            ILogger<WhatsAppDirectLineService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _conversationWatermarks = new ConcurrentDictionary<string, string>();
            _lastActivityTime = new ConcurrentDictionary<string, DateTime>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WhatsAppDirectLineService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PollActiveConversationsAsync();
                    await CleanupInactiveConversationsAsync();
                    await Task.Delay(_pollingInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in WhatsAppDirectLineService polling loop");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Wait before retrying
                }
            }

            _logger.LogInformation("WhatsAppDirectLineService stopped");
        }

        /// <summary>
        /// Polls all active DirectLine conversations for new bot responses
        /// </summary>
        private async Task PollActiveConversationsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var directLineService = scope.ServiceProvider.GetRequiredService<DirectLineService>();
            var whatsAppService = scope.ServiceProvider.GetRequiredService<WhatsAppApiService>();

            // Get all active conversations from DirectLineService
            var activeConversations = await GetActiveConversationsAsync(directLineService);

            foreach (var (phoneNumber, conversationId) in activeConversations)
            {
                try
                {
                    await PollConversationAsync(directLineService, whatsAppService, phoneNumber, conversationId);
                    _lastActivityTime[phoneNumber] = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error polling conversation {ConversationId} for {PhoneNumber}", 
                        conversationId, phoneNumber);
                }
            }
        }

        /// <summary>
        /// Polls a specific conversation for new activities
        /// </summary>
        private async Task PollConversationAsync(
            DirectLineService directLineService, 
            WhatsAppApiService whatsAppService,
            string phoneNumber, 
            string conversationId)
        {
            var watermark = _conversationWatermarks.GetValueOrDefault(conversationId);
            var activities = await directLineService.GetActivitiesAsync(conversationId, watermark);

            if (activities?.Activities == null || activities.Activities.Count == 0)
            {
                return;
            }

            // Update watermark
            if (!string.IsNullOrEmpty(activities.Watermark))
            {
                _conversationWatermarks[conversationId] = activities.Watermark;
            }

            // Process bot responses (exclude user messages)
            var botResponses = activities.Activities
                .Where(a => a.Type == ActivityTypes.Message && 
                           a.From?.Id != phoneNumber && // Not from the user
                           !string.IsNullOrEmpty(a.Text))
                .ToList();

            foreach (var response in botResponses)
            {
                try
                {
                    _logger.LogInformation("Sending bot response to WhatsApp user {PhoneNumber}: {Message}", 
                        phoneNumber, response.Text);

                    var success = await whatsAppService.SendTextMessageAsync(phoneNumber, response.Text!);
                    
                    if (success)
                    {
                        _logger.LogDebug("Bot response sent successfully to {PhoneNumber}", phoneNumber);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send bot response to {PhoneNumber}", phoneNumber);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending bot response to {PhoneNumber}", phoneNumber);
                }
            }
        }

        /// <summary>
        /// Gets active conversations from DirectLineService
        /// This is a simplified implementation - in production you'd want a more sophisticated approach
        /// </summary>
        private async Task<List<(string phoneNumber, string conversationId)>> GetActiveConversationsAsync(DirectLineService directLineService)
        {
            // This is a placeholder - in a real implementation, you'd need to track active conversations
            // For now, we'll use reflection to access the private conversation map
            // In production, you'd expose this through the DirectLineService interface
            
            var conversations = new List<(string phoneNumber, string conversationId)>();
            
            // TODO: Implement proper conversation tracking
            // For now, return empty list until we can properly track active conversations
            
            return conversations;
        }

        /// <summary>
        /// Cleans up inactive conversations to prevent memory leaks
        /// </summary>
        private async Task CleanupInactiveConversationsAsync()
        {
            var cutoffTime = DateTime.UtcNow - _conversationTimeout;
            var inactivePhones = _lastActivityTime
                .Where(kvp => kvp.Value < cutoffTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var phoneNumber in inactivePhones)
            {
                _lastActivityTime.TryRemove(phoneNumber, out _);
                
                // Also cleanup watermarks for this phone's conversation
                // This is simplified - in production you'd need proper phone->conversation mapping
                _logger.LogDebug("Cleaned up inactive conversation for {PhoneNumber}", phoneNumber);
            }

            if (inactivePhones.Count > 0)
            {
                _logger.LogInformation("Cleaned up {Count} inactive conversations", inactivePhones.Count);
            }
        }
    }
}