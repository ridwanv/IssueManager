using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IssueManager.Bot.Services;

namespace IssueManager.Bot.Middleware
{
    /// <summary>
    /// Middleware to verify WhatsApp webhook signatures for security
    /// </summary>
    public class WhatsAppSignatureVerificationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<WhatsAppSignatureVerificationMiddleware> _logger;
        private readonly IConfiguration _configuration;
        private readonly RateLimitingService _rateLimitingService;
        private readonly TimeSpan _maxTimestampAge;

        public WhatsAppSignatureVerificationMiddleware(RequestDelegate next, 
            ILogger<WhatsAppSignatureVerificationMiddleware> logger, 
            IConfiguration configuration,
            RateLimitingService rateLimitingService)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
            _rateLimitingService = rateLimitingService;
            _maxTimestampAge = TimeSpan.FromMinutes(configuration.GetValue<int>("WhatsApp:MaxTimestampAgeMinutes", 5));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only verify webhook endpoints
            if (context.Request.Path.StartsWithSegments("/webhook") && context.Request.Method == "POST")
            {
                // Check rate limiting first
                var clientIdentifier = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                if (!_rateLimitingService.IsInboundRequestAllowed(clientIdentifier))
                {
                    _logger.LogWarning("Rate limit exceeded for client {ClientId}", clientIdentifier);
                    context.Response.StatusCode = 429; // Too Many Requests
                    await context.Response.WriteAsync("Rate limit exceeded");
                    return;
                }

                // Verify signature and timestamp
                var verificationResult = await VerifyWhatsAppSignatureAndTimestamp(context);
                if (!verificationResult.IsValid)
                {
                    _logger.LogWarning("WhatsApp verification failed for request from {RemoteIp}: {Reason}", 
                        context.Connection.RemoteIpAddress, verificationResult.FailureReason);
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync($"Unauthorized - {verificationResult.FailureReason}");
                    return;
                }
            }

            await _next(context);
        }

        private async Task<VerificationResult> VerifyWhatsAppSignatureAndTimestamp(HttpContext context)
        {
            try
            {
                // Get the signature from the header
                if (!context.Request.Headers.TryGetValue("X-Hub-Signature-256", out var signatureHeader))
                {
                    return VerificationResult.Failure("Signature header missing");
                }

                var signature = signatureHeader.ToString();
                if (string.IsNullOrEmpty(signature) || !signature.StartsWith("sha256="))
                {
                    return VerificationResult.Failure("Invalid signature format");
                }

                // Check timestamp header for replay attack prevention
                if (context.Request.Headers.TryGetValue("X-WhatsApp-Timestamp", out var timestampHeader))
                {
                    if (long.TryParse(timestampHeader.ToString(), out var timestamp))
                    {
                        var requestTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                        var age = DateTimeOffset.UtcNow - requestTime;
                        
                        if (Math.Abs(age.TotalMilliseconds) > _maxTimestampAge.TotalMilliseconds)
                        {
                            return VerificationResult.Failure($"Request timestamp too old or future: {age.TotalMinutes:F1} minutes");
                        }
                    }
                }

                // Get the webhook secret from configuration
                var webhookSecret = Environment.GetEnvironmentVariable("WhatsApp__WebhookSecret") ?? 
                                   _configuration.GetValue<string>("WhatsApp:WebhookSecret");
                if (string.IsNullOrEmpty(webhookSecret))
                {
                    _logger.LogWarning("WhatsApp webhook secret not configured - skipping signature verification");
                    return VerificationResult.Success(); // Allow in development/testing scenarios
                }

                // Read the request body
                context.Request.EnableBuffering();
                string requestBody;
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                context.Request.Body.Position = 0; // Reset position for downstream processing

                // Calculate expected signature
                var expectedSignature = CalculateSignature(requestBody, webhookSecret);
                var receivedSignature = signature.Substring(7); // Remove "sha256=" prefix

                // Constant-time comparison to prevent timing attacks
                var isValid = CryptographicOperations.FixedTimeEquals(
                    Convert.FromHexString(expectedSignature),
                    Convert.FromHexString(receivedSignature)
                );

                return isValid ? VerificationResult.Success() : VerificationResult.Failure("Signature mismatch");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during WhatsApp signature verification");
                return VerificationResult.Failure("Verification error");
            }
        }

        private static string CalculateSignature(string payload, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }

    /// <summary>
    /// Result of signature and timestamp verification
    /// </summary>
    public class VerificationResult
    {
        public bool IsValid { get; private set; }
        public string FailureReason { get; private set; } = string.Empty;

        private VerificationResult(bool isValid, string failureReason = "")
        {
            IsValid = isValid;
            FailureReason = failureReason;
        }

        public static VerificationResult Success() => new(true);
        public static VerificationResult Failure(string reason) => new(false, reason);
    }
}