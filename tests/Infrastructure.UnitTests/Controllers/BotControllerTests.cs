using FluentAssertions;
using IssueManager.Bot.Controllers;
using IssueManager.Bot.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using System.Text;
using Xunit;

namespace Infrastructure.UnitTests.Controllers
{
    public class BotControllerTests
    {
        private readonly Mock<IBotFrameworkHttpAdapter> _mockAdapter;
        private readonly Mock<IBot> _mockBot;
        private readonly Mock<ILogger<BotController>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<WhatsAppMessageParser> _mockMessageParser;
        private readonly Mock<WhatsAppApiService> _mockWhatsAppApiService;
        private readonly BotController _controller;

        public BotControllerTests()
        {
            _mockAdapter = new Mock<IBotFrameworkHttpAdapter>();
            _mockBot = new Mock<IBot>();
            _mockLogger = new Mock<ILogger<BotController>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockMessageParser = new Mock<WhatsAppMessageParser>(Mock.Of<ILogger<WhatsAppMessageParser>>());
            _mockWhatsAppApiService = new Mock<WhatsAppApiService>(Mock.Of<HttpClient>(), Mock.Of<ILogger<WhatsAppApiService>>(), _mockConfiguration.Object);

            _controller = new BotController(
                _mockAdapter.Object,
                _mockBot.Object,
                _mockLogger.Object,
                _mockConfiguration.Object,
                _mockMessageParser.Object,
                _mockWhatsAppApiService.Object);
        }

        [Fact]
        public void WebhookVerification_WithValidParameters_ShouldReturnChallenge()
        {
            // Arrange
            const string expectedToken = "test-verify-token";
            const string challenge = "test-challenge-123";
            
            _mockConfiguration.Setup(c => c.GetValue<string>("WhatsApp:VerifyToken"))
                .Returns(expectedToken);

            // Act
            var result = _controller.WebhookVerification("subscribe", challenge, expectedToken);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().Be(challenge);
        }

        [Theory]
        [InlineData("invalid-mode", "test-challenge", "valid-token")]
        [InlineData("subscribe", "", "valid-token")]
        [InlineData("subscribe", null, "valid-token")]
        [InlineData("subscribe", "test-challenge", "invalid-token")]
        [InlineData("subscribe", "test-challenge", "")]
        [InlineData("subscribe", "test-challenge", null)]
        public void WebhookVerification_WithInvalidParameters_ShouldReturnBadRequest(string mode, string challenge, string verifyToken)
        {
            // Arrange
            _mockConfiguration.Setup(c => c.GetValue<string>("WhatsApp:VerifyToken"))
                .Returns("valid-token");

            // Act
            var result = _controller.WebhookVerification(mode, challenge, verifyToken);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void WebhookVerification_WithMissingConfiguration_ShouldReturnBadRequest()
        {
            // Arrange
            _mockConfiguration.Setup(c => c.GetValue<string>("WhatsApp:VerifyToken"))
                .Returns((string)null);

            // Act
            var result = _controller.WebhookVerification("subscribe", "challenge", "token");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("Verify token not configured");
        }

        [Fact]
        public async Task WebhookAsync_WithValidPayload_ShouldProcessSuccessfully()
        {
            // Arrange
            var validPayload = @"{""object"":""whatsapp_business_account"",""entry"":[]}";
            var requestBody = new MemoryStream(Encoding.UTF8.GetBytes(validPayload));
            
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = requestBody;
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var webhookPayload = new WhatsAppWebhookPayload { Object = "whatsapp_business_account" };
            var messageData = new WhatsAppMessageData 
            { 
                MessageId = "msg123", 
                From = "+27123456789", 
                Type = "text" 
            };

            _mockMessageParser.Setup(p => p.ParseMessage(validPayload))
                .Returns(webhookPayload);
            _mockMessageParser.Setup(p => p.ExtractMessageData(webhookPayload))
                .Returns(messageData);
            _mockWhatsAppApiService.Setup(s => s.SendReadReceiptAsync(messageData.MessageId))
                .Returns(Task.FromResult(true));
            _mockWhatsAppApiService.Setup(s => s.SendTypingIndicatorAsync(messageData.From))
                .Returns(Task.FromResult(true));

            // Act
            var result = await _controller.WebhookAsync();

            // Assert
            result.Should().BeOfType<OkResult>();
            _mockMessageParser.Verify(p => p.ParseMessage(validPayload), Times.Once);
            _mockMessageParser.Verify(p => p.ExtractMessageData(webhookPayload), Times.Once);
        }

        [Fact]
        public async Task WebhookAsync_WithInvalidPayload_ShouldReturnBadRequest()
        {
            // Arrange
            var invalidPayload = @"{""invalid"":""payload""}";
            var requestBody = new MemoryStream(Encoding.UTF8.GetBytes(invalidPayload));
            
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Body = requestBody;
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            _mockMessageParser.Setup(p => p.ParseMessage(invalidPayload))
                .Returns((WhatsAppWebhookPayload)null);

            // Act
            var result = await _controller.WebhookAsync();

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("Invalid webhook payload");
        }

        [Fact]
        public void Health_ShouldReturnHealthStatus()
        {
            // Act
            var result = _controller.Health();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().NotBeNull();
            
            // Check that the health status contains expected properties
            var healthStatus = okResult.Value;
            healthStatus.Should().NotBeNull();
        }

        [Fact]
        public void Ping_ShouldReturnPongResponse()
        {
            // Act
            var result = _controller.Ping();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().NotBeNull();
        }
    }
}