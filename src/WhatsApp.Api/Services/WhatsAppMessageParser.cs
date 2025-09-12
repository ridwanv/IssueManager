using System;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WhatsApp.Api.Models;

namespace WhatsApp.Api.Services
{
    /// <summary>
    /// Service for parsing WhatsApp Business API webhook messages
    /// </summary>
    public class WhatsAppMessageParser
    {
        private readonly ILogger<WhatsAppMessageParser> _logger;

        public WhatsAppMessageParser(ILogger<WhatsAppMessageParser> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Parses WhatsApp webhook payload
        /// </summary>
        /// <param name="payload">Raw JSON payload from WhatsApp webhook</param>
        /// <returns>Parsed webhook payload or null if invalid</returns>
        public WhatsAppWebhookPayload? ParseMessage(string payload)
        {
            try
            {
                if (string.IsNullOrEmpty(payload))
                {
                    _logger.LogWarning("Received empty webhook payload");
                    return null;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                };

                var webhookPayload = JsonSerializer.Deserialize<WhatsAppWebhookPayload>(payload, options);
                
                if (webhookPayload == null || string.IsNullOrEmpty(webhookPayload.Object))
                {
                    _logger.LogWarning("Invalid or missing webhook payload structure");
                    return null;
                }

                _logger.LogDebug("Successfully parsed WhatsApp webhook payload");
                return webhookPayload;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing WhatsApp webhook payload: {Payload}", payload);
                return null;
            }
        }

        /// <summary>
        /// Extracts message data from webhook payload
        /// </summary>
        /// <param name="payload">Parsed webhook payload</param>
        /// <returns>Message data or null if no valid message found</returns>
        public WhatsAppMessageData? ExtractMessageData(WhatsAppWebhookPayload payload)
        {
            try
            {
                if (payload?.Entry == null || payload.Entry.Count == 0)
                {
                    _logger.LogDebug("No entries found in webhook payload");
                    return null;
                }

                foreach (var entry in payload.Entry)
                {
                    if (entry.Changes == null) continue;

                    foreach (var change in entry.Changes)
                    {
                        if (change.Value?.Messages == null) continue;

                        foreach (var message in change.Value.Messages)
                        {
                            if (string.IsNullOrEmpty(message.From) || string.IsNullOrEmpty(message.Id))
                                continue;

                            var messageData = new WhatsAppMessageData
                            {
                                MessageId = message.Id,
                                From = message.From,
                                Type = message.Type ?? "unknown",
                                Text = message.Text?.Body ?? string.Empty,
                                Timestamp = message.Timestamp
                            };

                            _logger.LogDebug("Extracted message data: ID={MessageId}, From={From}, Type={Type}", 
                                messageData.MessageId, messageData.From, messageData.Type);

                            return messageData;
                        }
                    }
                }

                _logger.LogDebug("No valid message data found in webhook payload");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting message data from webhook payload");
                return null;
            }
        }
    }
}