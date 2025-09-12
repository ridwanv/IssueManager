#nullable enable
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System;
using Microsoft.Extensions.Configuration;
using WhatsApp.Api.Services;
using WhatsApp.Api.Models;

namespace WhatsApp.Api.Controllers
{
    /// <summary>
    /// WhatsApp to DirectLine bridge controller.
    /// Handles WhatsApp webhook and forwards messages to Azure Bot Service via DirectLine.
    /// Bot responses are handled asynchronously by WhatsAppDirectLineService.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class WhatsAppController : ControllerBase
    {
        private readonly ILogger<WhatsAppController> _logger;
        private readonly IConfiguration _configuration;
        private readonly WhatsAppMessageParser _messageParser;
        private readonly DirectLineService _directLineService;

        public WhatsAppController(
            ILogger<WhatsAppController> logger,
            IConfiguration configuration,
            WhatsAppMessageParser messageParser,
            DirectLineService directLineService)
        {
            _logger = logger;
            _configuration = configuration;
            _messageParser = messageParser;
            _directLineService = directLineService;
        }

        /// <summary>
        /// WhatsApp webhook verification endpoint for Meta WhatsApp Cloud API
        /// </summary>
        [HttpGet("webhook")]
        public IActionResult WebhookVerification([FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.challenge")] string challenge,
            [FromQuery(Name = "hub.verify_token")] string verifyToken)
        {
            try
            {
                _logger.LogInformation("Webhook verification request received from {RemoteIp}", 
                    Request.HttpContext.Connection.RemoteIpAddress);

                // Validate verify_token against configuration
                var expectedToken = Environment.GetEnvironmentVariable("WhatsApp__VerifyToken") ?? 
                                   _configuration.GetValue<string>("WhatsApp:VerifyToken");

                if (string.IsNullOrEmpty(expectedToken))
                {
                    _logger.LogWarning("WhatsApp verify token not configured");
                    return BadRequest("Verify token not configured");
                }

                if (mode == "subscribe" && !string.IsNullOrEmpty(challenge) && verifyToken == expectedToken)
                {
                    _logger.LogInformation("Webhook verification successful for token: {MaskedToken}", 
                        verifyToken?.Substring(0, Math.Min(4, verifyToken.Length)) + "***");
                    return Ok(challenge);
                }

                _logger.LogWarning("Webhook verification failed - invalid parameters. Mode: {Mode}, Challenge: {HasChallenge}, Token: {HasToken}", 
                    mode, !string.IsNullOrEmpty(challenge), !string.IsNullOrEmpty(verifyToken));
                return BadRequest("Verification failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during webhook verification");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// WhatsApp webhook endpoint - receives messages and forwards to DirectLine
        /// Returns immediately to satisfy WhatsApp webhook timeout requirements
        /// </summary>
        [HttpPost("webhook")]
        public async Task<IActionResult> WebhookAsync()
        {
            try
            {
                _logger.LogInformation("Webhook POST request received from {RemoteIp}", 
                    Request.HttpContext.Connection.RemoteIpAddress);

                // Read the request body
                string requestBody;
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                _logger.LogDebug("Webhook payload received, length: {Length}", requestBody?.Length ?? 0);

                // Parse WhatsApp message using the dedicated parser
                if (string.IsNullOrEmpty(requestBody))
                {
                    _logger.LogWarning("Received empty request body");
                    return BadRequest("Empty webhook payload");
                }

                var webhookPayload = _messageParser.ParseMessage(requestBody);
                if (webhookPayload == null)
                {
                    _logger.LogWarning("Failed to parse WhatsApp webhook payload");
                    return BadRequest("Invalid webhook payload");
                }

                // Extract message data for processing
                var messageData = _messageParser.ExtractMessageData(webhookPayload);
                if (messageData != null)
                {
                    _logger.LogInformation("Received WhatsApp message from {From}, Type: {Type}, MessageId: {MessageId}", 
                        messageData.From, messageData.Type, messageData.MessageId);

                    // Forward to DirectLine (fire and forget - background service handles responses)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _directLineService.SendMessageToBotAsync(messageData.From, messageData.Text ?? string.Empty);
                            _logger.LogDebug("Message forwarded to DirectLine for {From}", messageData.From);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error forwarding message to DirectLine for {From}", messageData.From);
                        }
                    });

                    _logger.LogInformation("WhatsApp message processed successfully from {From}", messageData.From);
                }

                // Return immediately to satisfy WhatsApp webhook timeout
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook request");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Health check endpoint for monitoring
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            try
            {
                var healthStatus = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Version = GetType().Assembly.GetName().Version?.ToString() ?? "Unknown",
                    Service = "WhatsApp DirectLine Bridge",
                    Uptime = Environment.TickCount64 / 1000
                };

                _logger.LogInformation("Health check requested - Status: {Status}", healthStatus.Status);
                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, new { Status = "Unhealthy", Error = "Health check failed" });
            }
        }
    }
}