using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WhatsApp.Api.Models
{
    /// <summary>
    /// WhatsApp webhook payload structure
    /// </summary>
    public class WhatsAppWebhookPayload
    {
        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;

        [JsonPropertyName("entry")]
        public List<WhatsAppEntry> Entry { get; set; } = new();
    }

    /// <summary>
    /// WhatsApp webhook entry
    /// </summary>
    public class WhatsAppEntry
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("changes")]
        public List<WhatsAppChange> Changes { get; set; } = new();
    }

    /// <summary>
    /// WhatsApp webhook change
    /// </summary>
    public class WhatsAppChange
    {
        [JsonPropertyName("value")]
        public WhatsAppChangeValue? Value { get; set; }

        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;
    }

    /// <summary>
    /// WhatsApp change value
    /// </summary>
    public class WhatsAppChangeValue
    {
        [JsonPropertyName("messaging_product")]
        public string MessagingProduct { get; set; } = string.Empty;

        [JsonPropertyName("metadata")]
        public WhatsAppMetadata? Metadata { get; set; }

        [JsonPropertyName("contacts")]
        public List<WhatsAppContact> Contacts { get; set; } = new();

        [JsonPropertyName("messages")]
        public List<WhatsAppMessage> Messages { get; set; } = new();
    }

    /// <summary>
    /// WhatsApp metadata
    /// </summary>
    public class WhatsAppMetadata
    {
        [JsonPropertyName("display_phone_number")]
        public string DisplayPhoneNumber { get; set; } = string.Empty;

        [JsonPropertyName("phone_number_id")]
        public string PhoneNumberId { get; set; } = string.Empty;
    }

    /// <summary>
    /// WhatsApp contact
    /// </summary>
    public class WhatsAppContact
    {
        [JsonPropertyName("profile")]
        public WhatsAppProfile? Profile { get; set; }

        [JsonPropertyName("wa_id")]
        public string WaId { get; set; } = string.Empty;
    }

    /// <summary>
    /// WhatsApp profile
    /// </summary>
    public class WhatsAppProfile
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// WhatsApp message
    /// </summary>
    public class WhatsAppMessage
    {
        [JsonPropertyName("context")]
        public WhatsAppMessageContext? Context { get; set; }

        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public WhatsAppTextMessage? Text { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }

    /// <summary>
    /// WhatsApp message context
    /// </summary>
    public class WhatsAppMessageContext
    {
        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    /// <summary>
    /// WhatsApp text message
    /// </summary>
    public class WhatsAppTextMessage
    {
        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;
    }

    /// <summary>
    /// Simplified message data for processing
    /// </summary>
    public class WhatsAppMessageData
    {
        public string MessageId { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
    }
}