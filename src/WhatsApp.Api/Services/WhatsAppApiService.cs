using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WhatsApp.Api.Services
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
            _apiVersion = _configuration.GetValue<string>("WhatsApp:ApiVersion") ?? "v20.0";

            var baseUrl = _configuration.GetValue<string>("WhatsApp:BaseUrl") ?? "https://graph.facebook.com";
            _httpClient.BaseAddress = new Uri($"{baseUrl}/{_apiVersion}/");
            
            if (!string.IsNullOrEmpty(_accessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
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
    }
}