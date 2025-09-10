using CleanArchitecture.Blazor.Application.Features.Conversations.Commands.AcceptEscalation;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CleanArchitecture.Blazor.Application.UnitTests.Features.Conversations.Commands;

public class AcceptEscalationCommandHandlerTests
{
    private readonly Mock<IApplicationDbContextFactory> _dbContextFactory;
    private readonly Mock<IUserContextAccessor> _userContextAccessor;
    private readonly Mock<IApplicationHubWrapper> _hubWrapper;
    private readonly AcceptEscalationCommandHandler _handler;

    public AcceptEscalationCommandHandlerTests()
    {
        _dbContextFactory = new Mock<IApplicationDbContextFactory>();
        _userContextAccessor = new Mock<IUserContextAccessor>();
        _hubWrapper = new Mock<IApplicationHubWrapper>();
        _handler = new AcceptEscalationCommandHandler(_dbContextFactory.Object, _userContextAccessor.Object, _hubWrapper.Object);
    }

    [Fact]
    public async Task Handle_WithNullRequest_ShouldReturnFailure()
    {
        // Act
        var result = await _handler.Handle(null!, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Invalid request", result.ErrorMessage);
    }

    [Fact]
    public async Task Handle_WithEmptyConversationId_ShouldReturnFailure()
    {
        // Arrange
        var request = new AcceptEscalationCommand { ConversationId = string.Empty };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Invalid request", result.ErrorMessage);
    }

    [Fact]
    public async Task Handle_WithNullUser_ShouldReturnFailure()
    {
        // Arrange
        var request = new AcceptEscalationCommand { ConversationId = "test-conv-123" };
        _userContextAccessor.Setup(x => x.Current).Returns((UserContext)null!);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("User not authenticated", result.ErrorMessage);
    }

    [Fact]
    public async Task Handle_WithNullUserId_ShouldReturnFailure()
    {
        // Arrange
        var request = new AcceptEscalationCommand { ConversationId = "test-conv-123" };
        var userContext = new UserContext("", "testuser");
        _userContextAccessor.Setup(x => x.Current).Returns(userContext);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("User not authenticated", result.ErrorMessage);
    }
}
