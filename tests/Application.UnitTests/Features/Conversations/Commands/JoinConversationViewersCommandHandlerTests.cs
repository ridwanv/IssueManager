using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Conversations.Commands.JoinConversationViewers;
using CleanArchitecture.Blazor.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CleanArchitecture.Blazor.Application.UnitTests.Features.Conversations.Commands;

public class JoinConversationViewersCommandHandlerTests
{
    private readonly Mock<IApplicationDbContextFactory> _dbContextFactory;
    private readonly JoinConversationViewersCommandHandler _handler;

    public JoinConversationViewersCommandHandlerTests()
    {
        _dbContextFactory = new Mock<IApplicationDbContextFactory>();
        _handler = new JoinConversationViewersCommandHandler(_dbContextFactory.Object);
    }

    [Fact]
    public async Task Handle_ValidConversationId_ReturnsSuccess()
    {
        // Arrange
        var conversationId = Guid.NewGuid().ToString();
        var userId = "test-user-id";
        var command = new JoinConversationViewersCommand(conversationId, userId);

        var conversations = new List<Conversation>
        {
            new Conversation
            {
                Id = 1, // Use an int value for Conversation.Id
                ConversationReference = "CONV-001",
                UserId = "customer-123",
                TenantId = "tenant-1"
            }
        };

        var mockContext = new Mock<IApplicationDbContext>();
        var mockDbSet = CreateMockDbSet(conversations);
        
        mockContext.Setup(c => c.Conversations).Returns(mockDbSet.Object);
        _dbContextFactory.Setup(f => f.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockContext.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_InvalidConversationId_ReturnsFailure()
    {
        // Arrange
        var command = new JoinConversationViewersCommand("invalid-guid", "user-id");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("Invalid conversation ID format.");
    }

    [Fact]
    public async Task Handle_ConversationNotFound_ReturnsFailure()
    {
        // Arrange
        var conversationId = Guid.NewGuid().ToString();
        var userId = "test-user-id";
        var command = new JoinConversationViewersCommand(conversationId, userId);

        var conversations = new List<Conversation>(); // Empty list

        var mockContext = new Mock<IApplicationDbContext>();
        var mockDbSet = CreateMockDbSet(conversations);
        
        mockContext.Setup(c => c.Conversations).Returns(mockDbSet.Object);
        _dbContextFactory.Setup(f => f.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockContext.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("Conversation not found or access denied.");
    }

    private Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> data) where T : class
    {
        var queryableData = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();
        
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableData.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator());
        
        return mockSet;
    }
}