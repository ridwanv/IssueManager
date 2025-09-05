using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CleanArchitecture.Blazor.Application.Features.Issues.Queries.GetIssueDetail;
using CleanArchitecture.Blazor.Application.Features.Issues.DTOs;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using Moq;
using FluentAssertions;
using Xunit;

namespace CleanArchitecture.Blazor.Application.UnitTests.Features.Issues.Queries;

public class GetIssueDetailQueryHandlerTests
{
    private readonly Mock<IApplicationDbContextFactory> _dbContextFactoryMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUserContextAccessor> _userContextAccessorMock;
    private readonly GetIssueDetailQueryHandler _handler;

    public GetIssueDetailQueryHandlerTests()
    {
        _dbContextFactoryMock = new Mock<IApplicationDbContextFactory>();
        _dbContextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _userContextAccessorMock = new Mock<IUserContextAccessor>();
        
        _dbContextFactoryMock.Setup(x => x.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_dbContextMock.Object);
            
        _handler = new GetIssueDetailQueryHandler(
            _dbContextFactoryMock.Object,
            _mapperMock.Object,
            _userContextAccessorMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenNoTenantContext_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetIssueDetailQuery { Id = Guid.NewGuid() };
        _userContextAccessorMock.Setup(x => x.Current).Returns((UserContext)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("Tenant context not found.");
    }

    [Fact]
    public async Task Handle_WhenIssueNotFound_ShouldReturnFailure()
    {
        // Arrange
        var issueId = Guid.NewGuid();
        var query = new GetIssueDetailQuery { Id = issueId };
        var userContext = new UserContext("user1", "testuser", TenantId: "tenant1");
        
        _userContextAccessorMock.Setup(x => x.Current).Returns(userContext);
        
        var issuesDbSet = new Mock<DbSet<Issue>>();
        var queryable = new List<Issue>().AsQueryable();
        
        issuesDbSet.As<IQueryable<Issue>>().Setup(m => m.Provider).Returns(queryable.Provider);
        issuesDbSet.As<IQueryable<Issue>>().Setup(m => m.Expression).Returns(queryable.Expression);
        issuesDbSet.As<IQueryable<Issue>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        issuesDbSet.As<IQueryable<Issue>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        
        _dbContextMock.Setup(x => x.Issues).Returns(issuesDbSet.Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain($"Issue with id: [{issueId}] not found.");
    }
}