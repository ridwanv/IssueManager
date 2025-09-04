using FluentAssertions;
using IssueManager.Bot.Middleware;
using IssueManager.Bot.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Infrastructure.UnitTests.Middleware
{
    public class WhatsAppSignatureVerificationMiddlewareTests
    {
        private readonly Mock<ILogger<WhatsAppSignatureVerificationMiddleware>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<RateLimitingService> _mockRateLimitingService;
        private readonly string _webhookSecret = "test-webhook-secret";

        public WhatsAppSignatureVerificationMiddlewareTests()
        {
            _mockLogger = new Mock<ILogger<WhatsAppSignatureVerificationMiddleware>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockRateLimitingService = new Mock<RateLimitingService>(
                Mock.Of<ILogger<RateLimitingService>>(), 
                _mockConfiguration.Object);

            _mockConfiguration.Setup(c => c.GetValue<string>("WhatsApp:WebhookSecret"))
                .Returns(_webhookSecret);
            _mockConfiguration.Setup(c => c.GetValue<int>("WhatsApp:MaxTimestampAgeMinutes", 5))
                .Returns(5);
        }

        [Fact]
        public async Task InvokeAsync_WithValidSignature_ShouldCallNext()
        {
            // Arrange
            var requestBody = @"{""object"":""whatsapp_business_account""}";
            var signature = CalculateSignature(requestBody, _webhookSecret);
            var context = CreateHttpContext("/webhook", "POST", requestBody);
            context.Request.Headers["X-Hub-Signature-256"] = $"sha256={signature}";
            
            _mockRateLimitingService.Setup(r => r.IsInboundRequestAllowed(It.IsAny<string>()))
                .Returns(true);

            var nextCalled = false;
            RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };
            
            var middleware = new WhatsAppSignatureVerificationMiddleware(
                next, _mockLogger.Object, _mockConfiguration.Object, _mockRateLimitingService.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            nextCalled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task InvokeAsync_WithInvalidSignature_ShouldReturn401()
        {
            // Arrange
            var requestBody = @"{""object"":""whatsapp_business_account""}";
            var invalidSignature = "invalid-signature";
            var context = CreateHttpContext("/webhook", "POST", requestBody);
            context.Request.Headers["X-Hub-Signature-256"] = $"sha256={invalidSignature}";
            
            _mockRateLimitingService.Setup(r => r.IsInboundRequestAllowed(It.IsAny<string>()))
                .Returns(true);

            var nextCalled = false;
            RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };
            
            var middleware = new WhatsAppSignatureVerificationMiddleware(
                next, _mockLogger.Object, _mockConfiguration.Object, _mockRateLimitingService.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            nextCalled.Should().BeFalse();
            context.Response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task InvokeAsync_WithMissingSignature_ShouldReturn401()
        {
            // Arrange
            var requestBody = @"{""object"":""whatsapp_business_account""}";
            var context = CreateHttpContext("/webhook", "POST", requestBody);
            
            _mockRateLimitingService.Setup(r => r.IsInboundRequestAllowed(It.IsAny<string>()))
                .Returns(true);

            var nextCalled = false;
            RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };
            
            var middleware = new WhatsAppSignatureVerificationMiddleware(
                next, _mockLogger.Object, _mockConfiguration.Object, _mockRateLimitingService.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            nextCalled.Should().BeFalse();
            context.Response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task InvokeAsync_WithRateLimitExceeded_ShouldReturn429()
        {
            // Arrange
            var requestBody = @"{""object"":""whatsapp_business_account""}";
            var context = CreateHttpContext("/webhook", "POST", requestBody);
            
            _mockRateLimitingService.Setup(r => r.IsInboundRequestAllowed(It.IsAny<string>()))
                .Returns(false);

            var nextCalled = false;
            RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };
            
            var middleware = new WhatsAppSignatureVerificationMiddleware(
                next, _mockLogger.Object, _mockConfiguration.Object, _mockRateLimitingService.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            nextCalled.Should().BeFalse();
            context.Response.StatusCode.Should().Be(429);
        }

        [Fact]
        public async Task InvokeAsync_WithNonWebhookPath_ShouldSkipValidation()
        {
            // Arrange
            var context = CreateHttpContext("/api/test", "GET", "");
            
            var nextCalled = false;
            RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };
            
            var middleware = new WhatsAppSignatureVerificationMiddleware(
                next, _mockLogger.Object, _mockConfiguration.Object, _mockRateLimitingService.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            nextCalled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task InvokeAsync_WithGetWebhook_ShouldSkipValidation()
        {
            // Arrange
            var context = CreateHttpContext("/webhook", "GET", "");
            
            var nextCalled = false;
            RequestDelegate next = (ctx) => { nextCalled = true; return Task.CompletedTask; };
            
            var middleware = new WhatsAppSignatureVerificationMiddleware(
                next, _mockLogger.Object, _mockConfiguration.Object, _mockRateLimitingService.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            nextCalled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(200);
        }

        private static HttpContext CreateHttpContext(string path, string method, string body)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = path;
            context.Request.Method = method;
            
            if (!string.IsNullOrEmpty(body))
            {
                var bodyBytes = Encoding.UTF8.GetBytes(body);
                context.Request.Body = new MemoryStream(bodyBytes);
                context.Request.ContentLength = bodyBytes.Length;
            }
            
            context.Response.Body = new MemoryStream();
            return context;
        }

        private static string CalculateSignature(string payload, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}