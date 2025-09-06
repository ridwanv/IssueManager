using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;
using CleanArchitecture.Blazor.Application.Features.Conversations.Commands.SendAgentMessage;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using CleanArchitecture.Blazor.Domain.Identity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore.Query;

namespace IssueManager.Application.UnitTests.Features.Conversations.Commands;

public class SendAgentMessageCommandHandlerTests
{
    private readonly Mock<IApplicationDbContextFactory> _mockDbContextFactory;
    private readonly Mock<IApplicationDbContext> _mockDbContext;
    private readonly Mock<IUserContextAccessor> _mockUserContextAccessor;
    private readonly Mock<IApplicationHubWrapper> _mockHubWrapper;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly SendAgentMessageCommandHandler _handler;

    public SendAgentMessageCommandHandlerTests()
    {
        _mockDbContextFactory = new Mock<IApplicationDbContextFactory>();
        _mockDbContext = new Mock<IApplicationDbContext>();
        _mockUserContextAccessor = new Mock<IUserContextAccessor>();
        _mockHubWrapper = new Mock<IApplicationHubWrapper>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        _handler = new SendAgentMessageCommandHandler(
            _mockDbContextFactory.Object,
            _mockUserContextAccessor.Object,
            _mockHubWrapper.Object,
            _mockHttpClientFactory.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_SendsMessageSuccessfully()
    {
        // Arrange
        var userId = "agent-123";
        var conversationId = 1;
        var messageContent = "Hello, I'm here to help!";

        var conversation = new Conversation
        {
            Id = conversationId,
            CurrentAgentId = userId,
            Status = ConversationStatus.Active,
            Mode = ConversationMode.Human,
            TenantId = "tenant-1",
            Participants = new List<ConversationParticipant>
            {
                new()
                {
                    Type = ParticipantType.Customer,
                    WhatsAppPhoneNumber = "+1234567890"
                }
            }
        };

        var agent = new Agent
        {
            ApplicationUserId = userId,
            ApplicationUser = new ApplicationUser
            {
                Id = userId,
                DisplayName = "Agent Smith",
                UserName = "agent.smith"
            }
        };

        SetupMocks(conversation, agent, userId);

        var command = new SendAgentMessageCommand
        {
            ConversationId = conversationId.ToString(),
            Content = messageContent
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        
        // Verify message was added to database
        _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        // Verify SignalR broadcast was called
        _mockHubWrapper.Verify(x => x.BroadcastNewConversationMessage(
            conversationId.ToString(), 
            "Agent Smith", 
            messageContent, 
            true), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotAuthenticated_ReturnsFailure()
    {
        // Arrange
        _mockUserContextAccessor.Setup(x => x.Current).Returns((UserContext?)null);
        _mockDbContextFactory.Setup(x => x.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockDbContext.Object);

        var command = new SendAgentMessageCommand
        {
            ConversationId = "1",
            Content = "Test message"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("User not authenticated.");
    }

    [Fact]
    public async Task Handle_ConversationNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = "agent-123";
        _mockUserContextAccessor.Setup(x => x.Current).Returns(new UserContext(userId, "testuser"));
        _mockDbContextFactory.Setup(x => x.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockDbContext.Object);

        var mockConversationsSet = CreateMockDbSet(new List<Conversation>());
        _mockDbContext.Setup(x => x.Conversations).Returns(mockConversationsSet.Object);

        var command = new SendAgentMessageCommand
        {
            ConversationId = "999",
            Content = "Test message"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("Conversation not found.");
    }

    [Fact]
    public async Task Handle_AgentNotAssignedToConversation_ReturnsFailure()
    {
        // Arrange
        var userId = "agent-123";
        var otherAgentId = "agent-456";
        var conversationId = 1;

        var conversation = new Conversation
        {
            Id = conversationId,
            CurrentAgentId = otherAgentId, // Different agent assigned
            Status = ConversationStatus.Active,
            Mode = ConversationMode.Human
        };

        SetupBasicMocks(conversation, userId);

        var command = new SendAgentMessageCommand
        {
            ConversationId = conversationId.ToString(),
            Content = "Test message"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("You are not assigned to this conversation.");
    }

    [Fact]
    public async Task Handle_AgentProfileNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = "agent-123";
        var conversationId = 1;

        var conversation = new Conversation
        {
            Id = conversationId,
            CurrentAgentId = userId,
            Status = ConversationStatus.Active,
            Mode = ConversationMode.Human
        };

        SetupBasicMocks(conversation, userId);

        // Setup empty agents collection
        var mockAgentsSet = CreateMockDbSet(new List<Agent>());
        _mockDbContext.Setup(x => x.Agents).Returns(mockAgentsSet.Object);

        var command = new SendAgentMessageCommand
        {
            ConversationId = conversationId.ToString(),
            Content = "Test message"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("Agent profile not found.");
    }

    private void SetupMocks(Conversation conversation, Agent agent, string userId)
    {
        _mockUserContextAccessor.Setup(x => x.Current).Returns(new UserContext(userId, "testuser"));
        _mockDbContextFactory.Setup(x => x.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockDbContext.Object);

        var mockConversationsSet = CreateMockDbSet(new List<Conversation> { conversation });
        var mockAgentsSet = CreateMockDbSet(new List<Agent> { agent });
        var mockMessagesSet = CreateMockDbSet(new List<ConversationMessage>());

        _mockDbContext.Setup(x => x.Conversations).Returns(mockConversationsSet.Object);
        _mockDbContext.Setup(x => x.Agents).Returns(mockAgentsSet.Object);
        _mockDbContext.Setup(x => x.ConversationMessages).Returns(mockMessagesSet.Object);

        // Setup HTTP client factory for Bot service calls
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Setup successful HTTP response
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
    }

    private void SetupBasicMocks(Conversation conversation, string userId)
    {
        _mockUserContextAccessor.Setup(x => x.Current).Returns(new UserContext(userId, "testuser"));
        _mockDbContextFactory.Setup(x => x.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockDbContext.Object);

        var mockConversationsSet = CreateMockDbSet(new List<Conversation> { conversation });
        _mockDbContext.Setup(x => x.Conversations).Returns(mockConversationsSet.Object);
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

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return new ValueTask();
    }
}

internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
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

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments().First();
        var executionResult = typeof(IQueryProvider)
            .GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: new[] { typeof(Expression) }
            )
            ?.MakeGenericMethod(expectedResultType)
            .Invoke(this, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
            ?.MakeGenericMethod(expectedResultType)
            .Invoke(null, new[] { executionResult });
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    { }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    { }



    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}