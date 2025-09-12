#nullable enable
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.Configuration;

namespace IssueManager.Bot.Controllers
{
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IBot _bot;
        private readonly ILogger<BotController> _logger;
        private readonly IConfiguration _configuration;

        public BotController(
            IBotFrameworkHttpAdapter adapter, 
            IBot bot, 
            ILogger<BotController> logger, 
            IConfiguration configuration)
        {
            _adapter = adapter;
            _bot = bot;
            _logger = logger;
            _configuration = configuration;
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

    }
}
