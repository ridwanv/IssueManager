using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using CleanArchitecture.Blazor.Domain.Events;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;

namespace CleanArchitecture.Blazor.Domain.UnitTests.Entities;

[TestFixture]
public class IssueTests
{
    [Test]
    public void Create_ShouldCreateIssueWithCorrectProperties()
    {
        // Arrange
        var referenceNumber = "ISS-2024-000001";
        var title = "Test Issue Title";
        var description = "Test issue description";
        var category = IssueCategory.Technical;
        var priority = IssuePriority.High;
        var contactId = 123;
        var tenantId = "tenant-123";
        var sourceMessageIds = "[\"msg1\", \"msg2\"]";
        var whatsAppMetadata = "{\"channel\":\"WhatsApp\"}";

        // Act
        var issue = Issue.Create(
            referenceNumber: referenceNumber,
            title: title,
            description: description,
            category: category,
            priority: priority,
            reporterContactId: contactId,
            tenantId: tenantId,
            sourceMessageIds: sourceMessageIds,
            whatsAppMetadata: whatsAppMetadata,
            consentFlag: true);

        // Assert
        issue.Id.Should().NotBe(Guid.Empty);
        issue.ReferenceNumber.Should().Be(referenceNumber);
        issue.Title.Should().Be(title);
        issue.Description.Should().Be(description);
        issue.Category.Should().Be(category);
        issue.Priority.Should().Be(priority);
        issue.Status.Should().Be(IssueStatus.New);
        issue.ReporterContactId.Should().Be(contactId);
        issue.TenantId.Should().Be(tenantId);
        issue.SourceMessageIds.Should().Be(sourceMessageIds);
        issue.WhatsAppMetadata.Should().Be(whatsAppMetadata);
        issue.ConsentFlag.Should().BeTrue();
        issue.Channel.Should().Be("WhatsApp");
        
        // Verify domain event was added
        issue.DomainEvents.Should().ContainSingle();
        issue.DomainEvents.First().Should().BeOfType<IssueCreatedEvent>();
        var domainEvent = issue.DomainEvents.First() as IssueCreatedEvent;
        domainEvent!.Item.Should().Be(issue);
    }

    [Test]
    public void Create_WithDefaults_ShouldUseDefaultValues()
    {
        // Arrange
        var referenceNumber = "ISS-2024-000001";
        var title = "Test Issue Title";
        var description = "Test issue description";
        var category = IssueCategory.General;
        var priority = IssuePriority.Medium;
        var tenantId = "tenant-123";

        // Act
        var issue = Issue.Create(
            referenceNumber: referenceNumber,
            title: title,
            description: description,
            category: category,
            priority: priority,
            reporterContactId: null,
            tenantId: tenantId);

        // Assert
        issue.SourceMessageIds.Should().Be("[]");
        issue.WhatsAppMetadata.Should().BeNull();
        issue.ConsentFlag.Should().BeTrue();
        issue.ReporterContactId.Should().BeNull();
    }

    [TestCase(IssueCategory.Technical)]
    [TestCase(IssueCategory.Billing)]
    [TestCase(IssueCategory.General)]
    [TestCase(IssueCategory.Feature)]
    public void Create_WithDifferentCategories_ShouldSetCategoryCorrectly(IssueCategory category)
    {
        // Arrange & Act
        var issue = Issue.Create(
            referenceNumber: "ISS-2024-000001",
            title: "Test Title",
            description: "Test Description",
            category: category,
            priority: IssuePriority.Medium,
            reporterContactId: null,
            tenantId: "tenant-123");

        // Assert
        issue.Category.Should().Be(category);
    }

    [TestCase(IssuePriority.Low)]
    [TestCase(IssuePriority.Medium)]
    [TestCase(IssuePriority.High)]
    [TestCase(IssuePriority.Critical)]
    public void Create_WithDifferentPriorities_ShouldSetPriorityCorrectly(IssuePriority priority)
    {
        // Arrange & Act
        var issue = Issue.Create(
            referenceNumber: "ISS-2024-000001",
            title: "Test Title",
            description: "Test Description",
            category: IssueCategory.General,
            priority: priority,
            reporterContactId: null,
            tenantId: "tenant-123");

        // Assert
        issue.Priority.Should().Be(priority);
    }
}
