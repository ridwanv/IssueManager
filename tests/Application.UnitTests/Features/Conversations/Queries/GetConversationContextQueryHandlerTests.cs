using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetConversationContext;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore.Query;

namespace IssueManager.Application.UnitTests.Features.Conversations.Queries;

public class GetConversationContextQueryHandlerTests
{
    private readonly Mock<IApplicationDbContextFactory> _mockDbContextFactory;
    private readonly Mock<IApplicationDbContext> _mockDbContext;
    private readonly GetConversationContextQueryHandler _handler;

    public GetConversationContextQueryHandlerTests()
    {
        _mockDbContextFactory = new Mock<IApplicationDbContextFactory>();
        _mockDbContext = new Mock<IApplicationDbContext>();
        _handler = new GetConversationContextQueryHandler(_mockDbContextFactory.Object);
    }

    [Fact]
    public async Task Handle_ValidConversation_ReturnsPopupDto()
    {
        // Arrange
        var conversationId = "1";
        var conversation = new Conversation
        {
            Id = 1,
            Created = DateTime.UtcNow.AddHours(-1),
            EscalatedAt = DateTime.UtcNow.AddMinutes(-30),
            EscalationReason = "Customer needs urgent help",
            Priority = 2,
            ConversationSummary = "Customer billing issue",
            Messages = new List<ConversationMessage>
            {
                new() { Content = "Hello, I need help", Timestamp = DateTime.UtcNow.AddMinutes(-35) },
                new() { Content = "Can you assist me?", Timestamp = DateTime.UtcNow.AddMinutes(-30) }
            },
            Participants = new List<ConversationParticipant>
            {
                new()
                {
                    Type = ParticipantType.Customer,
                    ParticipantName = "John Doe",
                    WhatsAppPhoneNumber = "+1234567890"
                }
            }
        };

        var mockConversationsSet = CreateMockDbSet(new[] { conversation });
        _mockDbContext.Setup(x => x.Conversations).Returns(mockConversationsSet.Object);
        _mockDbContextFactory.Setup(x => x.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockDbContext.Object);

        var query = new GetConversationContextQuery { ConversationId = conversationId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.ConversationReference.Should().Be(conversationId);
        result.Data.CustomerName.Should().Be("John Doe");
        result.Data.PhoneNumber.Should().Be("+1234567890");
        result.Data.EscalationReason.Should().Be("Customer needs urgent help");
        result.Data.Priority.Should().Be(2);
        result.Data.MessageCount.Should().Be(2);
        result.Data.ConversationSummary.Should().Be("Customer billing issue");
    }

    [Fact]
    public async Task Handle_ConversationNotFound_ReturnsFailure()
    {
        // Arrange
        var conversationId = 999;
        var mockConversationsSet = CreateMockDbSet(Array.Empty<Conversation>());
        _mockDbContext.Setup(x => x.Conversations).Returns(mockConversationsSet.Object);
        _mockDbContextFactory.Setup(x => x.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockDbContext.Object);

        var query = new GetConversationContextQuery { ConversationId = conversationId.ToString() };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("Conversation not found.");
    }

    [Fact]
    public async Task Handle_ConversationWithNoCustomerParticipant_UsesDefaultValues()
    {
        // Arrange
        var conversationId = 1;
        var conversation = new Conversation
        {
            Id = conversationId,
            Created = DateTime.UtcNow.AddHours(-1),
            EscalatedAt = DateTime.UtcNow.AddMinutes(-30),
            EscalationReason = "Test escalation",
            Priority = 1,
            Messages = new List<ConversationMessage>(),
            Participants = new List<ConversationParticipant>()
        };

        var mockConversationsSet = CreateMockDbSet(new[] { conversation });
        _mockDbContext.Setup(x => x.Conversations).Returns(mockConversationsSet.Object);
        _mockDbContextFactory.Setup(x => x.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockDbContext.Object);

        var query = new GetConversationContextQuery { ConversationId = conversationId.ToString() };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data!.CustomerName.Should().Be("Unknown Customer");
        result.Data.PhoneNumber.Should().Be("Unknown");
        result.Data.MessageCount.Should().Be(0);
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> data) where T : class
    {
        var queryableData = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();
        
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryableData.GetEnumerator()));

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryableData.Provider));

        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator());

        return mockSet;
    }
}

// Helper classes for async testing
public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        var result = Execute<TResult>(expression);
        return Task.FromResult(result);
    }

    TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }

    public TestAsyncEnumerable(Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }

    public T Current => _inner.Current;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
}