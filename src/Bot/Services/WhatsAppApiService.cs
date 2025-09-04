using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IssueManager.Bot.Services
{
    /// <summary>
    /// Service for communicating with WhatsApp Business API
    /// </summary>
    public class WhatsAppApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WhatsAppApiService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _accessToken;
        private readonly string _phoneNumberId;
        private readonly string _apiVersion;

        public WhatsAppApiService(HttpClient httpClient, ILogger<WhatsAppApiService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;

            _accessToken = Environment.GetEnvironmentVariable("WhatsApp__AccessToken") ?? 
                          _configuration.GetValue<string>("WhatsApp:AccessToken") ?? string.Empty;
            _phoneNumberId = Environment.GetEnvironmentVariable("WhatsApp__PhoneNumberId") ?? 
                           _configuration.GetValue<string>("WhatsApp:PhoneNumberId") ?? string.Empty;
            _apiVersion = _configuration.GetValue<string>("WhatsApp:ApiVersion") ?? "v18.0";

            var baseUrl = _configuration.GetValue<string>("WhatsApp:BaseUrl") ?? "https://graph.facebook.com";
            _httpClient.BaseAddress = new Uri($"{baseUrl}/{_apiVersion}/");
            
            if (!string.IsNullOrEmpty(_accessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            }
        }

        /// <summary>
        /// Sends read receipt acknowledgment to WhatsApp API
        /// </summary>
        /// <param name="messageId">Message ID to acknowledge</param>
        /// <returns>True if successful</returns>
        public async Task<bool> SendReadReceiptAsync(string messageId)
        {
            try
            {
                if (string.IsNullOrEmpty(_phoneNumberId))
                {
                    _logger.LogWarning("WhatsApp Phone Number ID not configured - skipping read receipt");
                    return false;
                }

                var payload = new
                {
                    messaging_product = "whatsapp",
                    status = "read",
                    message_id = messageId
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_phoneNumberId}/messages", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Read receipt sent successfully for message {MessageId}", messageId);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to send read receipt for message {MessageId}. Status: {StatusCode}, Response: {Response}", 
                    messageId, response.StatusCode, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending read receipt for message {MessageId}", messageId);
                return false;
            }
        }

        /// <summary>
        /// Sends typing indicator to WhatsApp API
        /// </summary>
        /// <param name="recipientPhoneNumber">Recipient's phone number</param>
        /// <returns>True if successful</returns>
        public async Task<bool> SendTypingIndicatorAsync(string recipientPhoneNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(_phoneNumberId))
                {
                    _logger.LogWarning("WhatsApp Phone Number ID not configured - skipping typing indicator");
                    return false;
                }

                var payload = new
                {
                    messaging_product = "whatsapp",
                    recipient_type = "individual",
                    to = recipientPhoneNumber,
                    type = "text",
                    text = new
                    {
                        preview_url = false,
                        body = "..."
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_phoneNumberId}/messages", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Typing indicator sent successfully to {Recipient}", recipientPhoneNumber);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to send typing indicator to {Recipient}. Status: {StatusCode}, Response: {Response}", 
                    recipientPhoneNumber, response.StatusCode, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending typing indicator to {Recipient}", recipientPhoneNumber);
                return false;
            }
        }

        /// <summary>
        /// Sends a text message to WhatsApp API
        /// </summary>
        /// <param name="recipientPhoneNumber">Recipient's phone number</param>
        /// <param name="messageText">Message text to send</param>
        /// <returns>True if successful</returns>
        public async Task<bool> SendTextMessageAsync(string recipientPhoneNumber, string messageText)
        {
            try
            {
                if (string.IsNullOrEmpty(_phoneNumberId))
                {
                    _logger.LogWarning("WhatsApp Phone Number ID not configured - skipping text message");
                    return false;
                }

                var payload = new
                {
                    messaging_product = "whatsapp",
                    recipient_type = "individual",
                    to = recipientPhoneNumber,
                    type = "text",
                    text = new
                    {
                        preview_url = false,
                        body = messageText
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_phoneNumberId}/messages", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Text message sent successfully to {Recipient}", recipientPhoneNumber);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to send text message to {Recipient}. Status: {StatusCode}, Response: {Response}", 
                    recipientPhoneNumber, response.StatusCode, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending text message to {Recipient}", recipientPhoneNumber);
                return false;
            }
        }

        /// <summary>
        /// Sends a welcome greeting message to a first-time user
        /// </summary>
        /// <param name="recipientPhoneNumber">Recipient's phone number</param>
        /// <returns>True if successful</returns>
        public async Task<bool> SendWelcomeMessageAsync(string recipientPhoneNumber)
        {
            var welcomeMessage = _configuration.GetValue<string>("LLM_WELCOME_MESSAGE") ?? 
                "👋 Hi! I'm here to help you log your issue or request. Please describe your problem and I'll create a ticket for you.";

            return await SendTextMessageAsync(recipientPhoneNumber, welcomeMessage);
        }
    }
}