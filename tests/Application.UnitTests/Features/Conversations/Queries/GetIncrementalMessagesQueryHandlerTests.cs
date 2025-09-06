using AutoMapper;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;
using CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetIncrementalMessages;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CleanArchitecture.Blazor.Application.UnitTests.Features.Conversations.Queries;

public class GetIncrementalMessagesQueryHandlerTests
{
    private readonly Mock<IApplicationDbContextFactory> _dbContextFactory;
    private readonly Mock<IMapper> _mapper;
    private readonly GetIncrementalMessagesQueryHandler _handler;

    public GetIncrementalMessagesQueryHandlerTests()
    {
        _dbContextFactory = new Mock<IApplicationDbContextFactory>();
        _mapper = new Mock<IMapper>();
        _handler = new GetIncrementalMessagesQueryHandler(_dbContextFactory.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_ValidConversationId_ReturnsIncrementalMessages()
    {
        // Arrange
        var conversationId = Guid.NewGuid().ToString();
        var lastTimestamp = DateTime.UtcNow.AddMinutes(-10);
        var query = new GetIncrementalMessagesQuery(conversationId, lastTimestamp);

        var messages = new List<ConversationMessage>
        {
            new ConversationMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = Guid.Parse(conversationId),
                Content = "New message",
                Timestamp = DateTime.UtcNow.AddMinutes(-5),
                Role = "user"
            }
        };

        var messageDtos = new List<ConversationMessageDto>
        {
            new ConversationMessageDto
            {
                Content = "New message",
                Timestamp = DateTime.UtcNow.AddMinutes(-5),
                Role = "user"
            }
        };

        var mockContext = new Mock<IApplicationDbContext>();
        var mockDbSet = CreateMockDbSet(messages);
        
        mockContext.Setup(c => c.ConversationMessages).Returns(mockDbSet.Object);
        _dbContextFactory.Setup(f => f.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockContext.Object);
        
        _mapper.Setup(m => m.Map<List<ConversationMessageDto>>(It.IsAny<List<ConversationMessage>>()))
            .Returns(messageDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data.First().Content.Should().Be("New message");
    }

    [Fact]
    public async Task Handle_InvalidConversationId_ReturnsFailure()
    {
        // Arrange
        var query = new GetIncrementalMessagesQuery("invalid-guid");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("Invalid conversation ID format.");
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