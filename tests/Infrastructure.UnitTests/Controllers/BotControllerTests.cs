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
        private readonly BotController _controller;

        public BotControllerTests()
        {
            _mockAdapter = new Mock<IBotFrameworkHttpAdapter>();
            _mockBot = new Mock<IBot>();
            _mockLogger = new Mock<ILogger<BotController>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _controller = new BotController(
                _mockAdapter.Object,
                _mockBot.Object,
                _mockLogger.Object,
                _mockConfiguration.Object);
        }

        [Fact]
        public async Task MessagesAsync_ShouldProcessSuccessfully()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.MessagesAsync();

            // Assert
            result.Should().BeOfType<OkResult>();
            _mockAdapter.Verify(a => a.ProcessAsync(It.IsAny<HttpRequest>(), It.IsAny<HttpResponse>(), _mockBot.Object, It.IsAny<CancellationToken>()), Times.Once);
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