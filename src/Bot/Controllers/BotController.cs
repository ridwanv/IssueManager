using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using IssueManager.Bot.Services;

namespace IssueManager.Bot.Controllers
{
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IBot _bot;
        private readonly ILogger<BotController> _logger;
        private readonly IConfiguration _configuration;
        private readonly WhatsAppMessageParser _messageParser;
        private readonly WhatsAppApiService _whatsAppApiService;

        public BotController(
            IBotFrameworkHttpAdapter adapter, 
            IBot bot, 
            ILogger<BotController> logger, 
            IConfiguration configuration,
            WhatsAppMessageParser messageParser,
            WhatsAppApiService whatsAppApiService)
        {
            _adapter = adapter;
            _bot = bot;
            _logger = logger;
            _configuration = configuration;
            _messageParser = messageParser;
            _whatsAppApiService = whatsAppApiService;
        }

        /// <summary>
        /// WhatsApp webhook endpoint for receiving messages
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
                    _logger.LogInformation("Processing WhatsApp message from {From}, Type: {Type}", 
                        messageData.From, messageData.Type);

                    // Send read receipt acknowledgment
                    _ = Task.Run(async () =>
                    {
                        await _whatsAppApiService.SendReadReceiptAsync(messageData.MessageId);
                    });

                    // Send typing indicator while processing
                    _ = Task.Run(async () =>
                    {
                        await _whatsAppApiService.SendTypingIndicatorAsync(messageData.From);
                    });
                }

                // Reset the request body stream for bot framework processing
                Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

                // Delegate to bot framework adapter for conversational AI processing
                await _adapter.ProcessAsync(Request, Response, _bot);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook request");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// WhatsApp webhook verification endpoint
        /// </summary>
        [HttpGet("webhook")]
        public IActionResult WebhookVerification([FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.challenge")] string challenge,
            [FromQuery(Name = "hub.verify_token")] string verifyToken)
        {
            try
            {
                _logger.LogInformation("Webhook verification request received from {RemoteIp}", Request.HttpContext.Connection.RemoteIpAddress);

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
        /// Legacy Bot Framework messages endpoint
        /// </summary>
        [HttpPost("api/messages")]
        [HttpGet("api/messages")]
        public async Task<IActionResult> MessagesAsync()
        {
            try
            {
                await _adapter.ProcessAsync(Request, Response, _bot);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bot framework message");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Health check endpoint for service monitoring
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
                    Service = "WhatsApp Bot Service",
                    Uptime = Environment.TickCount64 / 1000 // seconds since process start
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

        /// <summary>
        /// Simple ping endpoint for basic connectivity testing
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            try
            {
                var response = new
                {
                    Message = "Pong",
                    Timestamp = DateTime.UtcNow,
                    Server = Environment.MachineName
                };

                _logger.LogDebug("Ping request received and responded");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ping endpoint failed");
                return StatusCode(500, "Ping failed");
            }
        }

        /// <summary>
        /// Endpoint for sending agent messages via WhatsApp
        /// </summary>
        [HttpPost("api/agent-message")]
        public async Task<IActionResult> SendAgentMessageAsync([FromBody] AgentMessageRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.PhoneNumber) || 
                    string.IsNullOrEmpty(request.Message) || request.ConversationId <= 0)
                {
                    _logger.LogWarning("Invalid agent message request - missing required fields");
                    return BadRequest("PhoneNumber, Message, and ConversationId are required");
                }

                _logger.LogInformation("Sending agent message for conversation {ConversationId} to {PhoneNumber}", 
                    request.ConversationId, request.PhoneNumber);

                var success = await _whatsAppApiService.SendTextMessageAsync(request.PhoneNumber, request.Message);

                if (success)
                {
                    _logger.LogInformation("Agent message sent successfully for conversation {ConversationId} to {PhoneNumber}", 
                        request.ConversationId, request.PhoneNumber);
                    
                    return Ok(new { 
                        Success = true, 
                        Message = "Agent message sent successfully",
                        ConversationId = request.ConversationId 
                    });
                }
                else
                {
                    _logger.LogWarning("Failed to send agent message for conversation {ConversationId} to {PhoneNumber}", 
                        request.ConversationId, request.PhoneNumber);
                    return StatusCode(500, new { 
                        Success = false, 
                        Message = "Failed to send agent message",
                        ConversationId = request.ConversationId 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending agent message for conversation {ConversationId} to {PhoneNumber}", 
                    request?.ConversationId, request?.PhoneNumber);
                return StatusCode(500, new { 
                    Success = false, 
                    Message = "Internal server error",
                    ConversationId = request?.ConversationId 
                });
            }
        }

        /// <summary>
        /// Endpoint for sending proactive WhatsApp messages
        /// </summary>
        [HttpPost("api/proactive-message")]
        public async Task<IActionResult> SendProactiveMessageAsync([FromBody] ProactiveMessageRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.Message))
                {
                    _logger.LogWarning("Invalid proactive message request - missing required fields");
                    return BadRequest("PhoneNumber and Message are required");
                }

                _logger.LogInformation("Sending proactive message to {PhoneNumber}", request.PhoneNumber);

                var success = await _whatsAppApiService.SendTextMessageAsync(request.PhoneNumber, request.Message);

                if (success)
                {
                    _logger.LogInformation("Proactive message sent successfully to {PhoneNumber}", request.PhoneNumber);
                    return Ok(new { Success = true, Message = "Proactive message sent successfully" });
                }
                else
                {
                    _logger.LogWarning("Failed to send proactive message to {PhoneNumber}", request.PhoneNumber);
                    return StatusCode(500, new { Success = false, Message = "Failed to send proactive message" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending proactive message to {PhoneNumber}", request?.PhoneNumber);
                return StatusCode(500, new { Success = false, Message = "Internal server error" });
            }
        }
    }

    /// <summary>
    /// Request model for agent messages
    /// </summary>
    public class AgentMessageRequest
    {
        public int ConversationId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string AgentName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for proactive messaging
    /// </summary>
    public class ProactiveMessageRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
