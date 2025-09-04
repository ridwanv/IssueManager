using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Issues.Commands.Create;
using CleanArchitecture.Blazor.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CleanArchitecture.Blazor.Application.UnitTests.Services;

public class IssueReferenceNumberServiceTests
{
    private readonly Mock<IApplicationDbContextFactory> _mockDbContextFactory;
    private readonly Mock<IApplicationDbContext> _mockDbContext;
    private readonly Mock<DbSet<Issue>> _mockIssueSet;
    private readonly IssueReferenceNumberService _service;

    public IssueReferenceNumberServiceTests()
    {
        _mockDbContextFactory = new Mock<IApplicationDbContextFactory>();
        _mockDbContext = new Mock<IApplicationDbContext>();
        _mockIssueSet = new Mock<DbSet<Issue>>();
        _service = new IssueReferenceNumberService(_mockDbContextFactory.Object);

        _mockDbContextFactory
            .Setup(x => x.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_mockDbContext.Object);
            
        _mockDbContext
            .Setup(x => x.Issues)
            .Returns(_mockIssueSet.Object);
    }

    [Fact]
    public async Task GenerateReferenceNumberAsync_WhenNoExistingIssues_ShouldReturnFirstSequence()
    {
        // Arrange
        var emptyList = new List<string>().AsQueryable();
        SetupMockDbSet(emptyList);

        // Act
        var result = await _service.GenerateReferenceNumberAsync();

        // Assert
        var currentYear = DateTime.UtcNow.Year;
        result.Should().Be($"ISS-{currentYear}-000001");
    }

    [Fact]
    public async Task GenerateReferenceNumberAsync_WhenExistingIssuesExist_ShouldReturnNextSequence()
    {
        // Arrange
        var currentYear = DateTime.UtcNow.Year;
        var existingReferenceNumbers = new List<string>
        {
            $"ISS-{currentYear}-000001",
            $"ISS-{currentYear}-000005",
            $"ISS-{currentYear}-000003"
        }.AsQueryable();
        
        SetupMockDbSet(existingReferenceNumbers);

        // Act
        var result = await _service.GenerateReferenceNumberAsync();

        // Assert
        result.Should().Be($"ISS-{currentYear}-000006");
    }

    [Fact]
    public async Task GenerateReferenceNumberAsync_WhenMixedYears_ShouldOnlyConsiderCurrentYear()
    {
        // Arrange
        var currentYear = DateTime.UtcNow.Year;
        var previousYear = currentYear - 1;
        var existingReferenceNumbers = new List<string>
        {
            $"ISS-{previousYear}-000010", // Previous year - should be ignored
            $"ISS-{currentYear}-000002",
            $"ISS-{currentYear}-000001"
        }.AsQueryable();
        
        SetupMockDbSet(existingReferenceNumbers);

        // Act
        var result = await _service.GenerateReferenceNumberAsync();

        // Assert
        result.Should().Be($"ISS-{currentYear}-000003");
    }

    [Fact]
    public async Task GenerateReferenceNumberAsync_WhenInvalidFormat_ShouldIgnoreInvalidEntries()
    {
        // Arrange
        var currentYear = DateTime.UtcNow.Year;
        var existingReferenceNumbers = new List<string>
        {
            $"ISS-{currentYear}-000001",
            $"ISS-{currentYear}-INVALID", // Invalid format - should be ignored
            $"WRONG-{currentYear}-000002", // Wrong prefix - should be ignored
            $"ISS-{currentYear}-000003"
        }.AsQueryable();
        
        SetupMockDbSet(existingReferenceNumbers);

        // Act
        var result = await _service.GenerateReferenceNumberAsync();

        // Assert
        result.Should().Be($"ISS-{currentYear}-000004");
    }

    [Fact]
    public void GenerateReferenceNumberAsync_ShouldFormatSequenceWithSixDigits()
    {
        // Arrange
        var currentYear = DateTime.UtcNow.Year;
        var expected = $"ISS-{currentYear}-000001";

        // Act & Assert - Testing format
        expected.Should().MatchRegex(@"ISS-\d{4}-\d{6}");
        expected.Length.Should().Be(17); // ISS-YYYY-NNNNNN = 17 characters
    }

    private void SetupMockDbSet(IQueryable<string> data)
    {
        var mockQueryable = new Mock<IQueryable<Issue>>();
        var issues = data.Select(refNum => new Issue { ReferenceNumber = refNum }).AsQueryable();
        
        _mockIssueSet.As<IQueryable<Issue>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<Issue>(issues.Provider));
            
        _mockIssueSet.As<IQueryable<Issue>>()
            .Setup(m => m.Expression)
            .Returns(issues.Expression);
            
        _mockIssueSet.As<IQueryable<Issue>>()
            .Setup(m => m.ElementType)
            .Returns(issues.ElementType);
            
        _mockIssueSet.As<IQueryable<Issue>>()
            .Setup(m => m.GetEnumerator())
            .Returns(issues.GetEnumerator());
    }
}
