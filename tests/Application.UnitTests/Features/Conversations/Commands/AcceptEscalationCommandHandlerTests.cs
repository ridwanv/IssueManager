using CleanArchitecture.Blazor.Application.Features.Conversations.Commands.AcceptEscalation;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace CleanArchitecture.Blazor.Application.UnitTests.Features.Conversations.Commands;

public class AcceptEscalationCommandHandlerTests
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IUserContextAccessor _userContextAccessor;
    private readonly IApplicationHubWrapper _hubWrapper;
    private readonly AcceptEscalationCommandHandler _handler;

    public AcceptEscalationCommandHandlerTests()
    {
        _dbContextFactory = Substitute.For<IApplicationDbContextFactory>();
        _userContextAccessor = Substitute.For<IUserContextAccessor>();
        _hubWrapper = Substitute.For<IApplicationHubWrapper>();
        _handler = new AcceptEscalationCommandHandler(_dbContextFactory, _userContextAccessor, _hubWrapper);
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
        _userContextAccessor.Current.Returns((UserContext)null!);

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
        var userContext = new UserContext { UserId = null };
        _userContextAccessor.Current.Returns(userContext);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("User not authenticated", result.ErrorMessage);
    }
}
