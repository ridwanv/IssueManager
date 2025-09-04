using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace IssueManager.Bot.Services
{
    /// <summary>
    /// Service for parsing and validating WhatsApp Business API messages
    /// </summary>
    public class WhatsAppMessageParser
    {
        private readonly ILogger<WhatsAppMessageParser> _logger;

        public WhatsAppMessageParser(ILogger<WhatsAppMessageParser> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Parses WhatsApp webhook payload and validates message structure
        /// </summary>
        /// <param name="payload">Raw JSON payload from WhatsApp webhook</param>
        /// <returns>Parsed message data or null if invalid</returns>
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
                
                if (webhookPayload == null)
                {
                    _logger.LogWarning("Failed to deserialize webhook payload");
                    return null;
                }

                // Validate required fields
                if (!ValidateWebhookPayload(webhookPayload))
                {
                    _logger.LogWarning("Webhook payload validation failed");
                    return null;
                }

                _logger.LogInformation("Successfully parsed WhatsApp message from {PhoneNumber}", 
                    webhookPayload.Entry?.FirstOrDefault()?.Changes?.FirstOrDefault()?.Value?.Messages?.FirstOrDefault()?.From ?? "unknown");

                return webhookPayload;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse WhatsApp webhook JSON payload");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error parsing WhatsApp webhook payload");
                return null;
            }
        }

        /// <summary>
        /// Extracts message information from parsed webhook payload
        /// </summary>
        public WhatsAppMessageData? ExtractMessageData(WhatsAppWebhookPayload payload)
        {
            try
            {
                var entry = payload.Entry?.FirstOrDefault();
                if (entry == null)
                {
                    _logger.LogWarning("No entry found in webhook payload");
                    return null;
                }

                var change = entry.Changes?.FirstOrDefault();
                if (change == null)
                {
                    _logger.LogWarning("No changes found in webhook entry");
                    return null;
                }

                var message = change.Value?.Messages?.FirstOrDefault();
                if (message == null)
                {
                    _logger.LogDebug("No messages found in webhook change - might be status update");
                    return null;
                }

                return new WhatsAppMessageData
                {
                    MessageId = message.Id,
                    From = message.From,
                    Timestamp = message.Timestamp,
                    Type = message.Type,
                    Text = message.Text?.Body,
                    BusinessAccountId = entry.Id,
                    DisplayPhoneNumber = change.Value.Metadata?.DisplayPhoneNumber,
                    PhoneNumberId = change.Value.Metadata?.PhoneNumberId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting message data from webhook payload");
                return null;
            }
        }

        private bool ValidateWebhookPayload(WhatsAppWebhookPayload payload)
        {
            var context = new ValidationContext(payload);
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            return Validator.TryValidateObject(payload, context, results, true);
        }
    }

    /// <summary>
    /// WhatsApp webhook payload structure
    /// </summary>
    public class WhatsAppWebhookPayload
    {
        [Required]
        public string Object { get; set; } = string.Empty;
        
        [Required]
        public List<WhatsAppEntry> Entry { get; set; } = new();
    }

    public class WhatsAppEntry
    {
        [Required]
        public string Id { get; set; } = string.Empty;
        
        public List<WhatsAppChange> Changes { get; set; } = new();
    }

    public class WhatsAppChange
    {
        [Required]
        public string Field { get; set; } = string.Empty;
        
        public WhatsAppChangeValue Value { get; set; } = new();
    }

    public class WhatsAppChangeValue
    {
        public string MessagingProduct { get; set; } = string.Empty;
        public WhatsAppMetadata Metadata { get; set; } = new();
        public List<WhatsAppMessage> Messages { get; set; } = new();
        public List<WhatsAppStatus> Statuses { get; set; } = new();
    }

    public class WhatsAppMetadata
    {
        public string DisplayPhoneNumber { get; set; } = string.Empty;
        public string PhoneNumberId { get; set; } = string.Empty;
    }

    public class WhatsAppMessage
    {
        [Required]
        public string Id { get; set; } = string.Empty;
        
        [Required]
        public string From { get; set; } = string.Empty;
        
        [Required]
        public string Timestamp { get; set; } = string.Empty;
        
        [Required]
        public string Type { get; set; } = string.Empty;
        
        public WhatsAppTextMessage? Text { get; set; }
        public WhatsAppImageMessage? Image { get; set; }
        public WhatsAppDocumentMessage? Document { get; set; }
    }

    public class WhatsAppTextMessage
    {
        [Required]
        public string Body { get; set; } = string.Empty;
    }

    public class WhatsAppImageMessage
    {
        public string Id { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public string Sha256 { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
    }

    public class WhatsAppDocumentMessage
    {
        public string Id { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public string Sha256 { get; set; } = string.Empty;
    }

    public class WhatsAppStatus
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string RecipientId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Simplified message data for processing
    /// </summary>
    public class WhatsAppMessageData
    {
        public string MessageId { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Text { get; set; }
        public string BusinessAccountId { get; set; } = string.Empty;
        public string? DisplayPhoneNumber { get; set; }
        public string? PhoneNumberId { get; set; }
    }
}