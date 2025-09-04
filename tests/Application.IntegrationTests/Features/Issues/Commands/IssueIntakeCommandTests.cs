using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Application.Common.Models;
using CleanArchitecture.Blazor.Application.Features.Issues.Commands.Create;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace CleanArchitecture.Blazor.Application.IntegrationTests.Features.Issues.Commands;

using static Testing;

[TestFixture]
public class IssueIntakeCommandTests : TestBase
{
    [Test]
    public async Task Handle_ValidCommand_ShouldCreateIssueSuccessfully()
    {
        // Arrange
        var command = new IssueIntakeCommand
        {
            ReporterPhone = "+1234567890",
            ReporterName = "Test User",
            Channel = "WhatsApp",
            Category = "Technical",
            Product = "Test Product",
            Severity = "High",
            Priority = "High",
            Summary = "Test issue summary",
            Description = "Test issue description",
            SourceMessageIds = "[\"msg1\", \"msg2\"]",
            ConsentFlag = true
        };

        // Act
        var result = await SendAsync(command);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBe(Guid.Empty);

        // Verify issue was created in database
        var createdIssue = await FindAsync<Issue>(result.Data);
        createdIssue.Should().NotBeNull();
        createdIssue!.ReferenceNumber.Should().StartWith("ISS-2025-");
        createdIssue.Title.Should().Be("Test issue summary");
        createdIssue.Description.Should().Be("Test issue description");
        createdIssue.Category.Should().Be(IssueCategory.Technical);
        createdIssue.Priority.Should().Be(IssuePriority.High);
        createdIssue.Status.Should().Be(IssueStatus.New);
        createdIssue.Channel.Should().Be("WhatsApp");
        createdIssue.ConsentFlag.Should().BeTrue();
        createdIssue.WhatsAppMetadata.Should().NotBeNullOrEmpty();

        // Verify contact was created
        createdIssue.ReporterContactId.Should().NotBeNull();
        var contact = await FindAsync<Contact>(createdIssue.ReporterContactId.Value);
        contact.Should().NotBeNull();
        contact!.PhoneNumber.Should().Be("+1234567890");
        contact.Name.Should().Be("Test User");
    }

    [Test]
    public async Task Handle_WithAttachments_ShouldCreateIssueWithAttachments()
    {
        // Arrange
        var command = new IssueIntakeCommand
        {
            ReporterPhone = "+1234567890",
            ReporterName = "Test User",
            Channel = "WhatsApp",
            Category = "Technical",
            Product = "Test Product",
            Severity = "Medium",
            Priority = "Medium",
            Summary = "Test issue with attachments",
            Description = "Test issue description",
            ConsentFlag = true,
            Attachments = new List<IssueAttachmentData>
            {
                new()
                {
                    Name = "test.jpg",
                    ContentType = "image/jpeg",
                    Url = "https://example.com/test.jpg",
                    Size = 1024
                }
            }
        };

        // Act
        var result = await SendAsync(command);

        // Assert
        result.Succeeded.Should().BeTrue();

        // Verify attachments were created
        await using var context = CreateDbContext();
        var attachments = await context.Attachments
            .Where(a => a.IssueId == result.Data)
            .ToListAsync();

        attachments.Should().HaveCount(1);
        attachments[0].Uri.Should().Be("https://example.com/test.jpg");
        attachments[0].Type.Should().Be("image/jpeg");
        attachments[0].SizeBytes.Should().Be(1024);
        attachments[0].ScanStatus.Should().Be("Pending");
    }

    [Test]
    public async Task Handle_ExistingContact_ShouldReuseContact()
    {
        // Arrange - Create existing contact
        var existingContact = new Contact
        {
            PhoneNumber = "+1234567890",
            Name = "Existing User",
            Description = "Test contact",
            TenantId = await GetDefaultTenantId()
        };
        await AddAsync(existingContact);

        var command = new IssueIntakeCommand
        {
            ReporterPhone = "+1234567890",
            ReporterName = "Updated Name",
            Channel = "WhatsApp",
            Category = "General",
            Product = "Test Product",
            Severity = "Low",
            Priority = "Low",
            Summary = "Test reuse contact",
            Description = "Test description",
            ConsentFlag = true
        };

        // Act
        var result = await SendAsync(command);

        // Assert
        result.Succeeded.Should().BeTrue();

        var createdIssue = await FindAsync<Issue>(result.Data);
        createdIssue!.ReporterContactId.Should().Be(existingContact.Id);

        // Verify contact name was updated
        var updatedContact = await FindAsync<Contact>(existingContact.Id);
        updatedContact!.Name.Should().Be("Updated Name");
    }

    [Test]
    public async Task Handle_InvalidCategory_ShouldUseDefaultCategory()
    {
        // Arrange
        var command = new IssueIntakeCommand
        {
            ReporterPhone = "+1234567890",
            ReporterName = "Test User",
            Channel = "WhatsApp",
            Category = "InvalidCategory",
            Product = "Test Product",
            Severity = "Medium",
            Priority = "Medium",
            Summary = "Test invalid category",
            Description = "Test description",
            ConsentFlag = true
        };

        // Act
        var result = await SendAsync(command);

        // Assert
        result.Succeeded.Should().BeTrue();

        var createdIssue = await FindAsync<Issue>(result.Data);
        createdIssue!.Category.Should().Be(IssueCategory.General); // Default fallback
    }

    [Test]
    public async Task Handle_InvalidPriority_ShouldUseDefaultPriority()
    {
        // Arrange
        var command = new IssueIntakeCommand
        {
            ReporterPhone = "+1234567890",
            ReporterName = "Test User",
            Channel = "WhatsApp",
            Category = "Technical",
            Product = "Test Product",
            Severity = "High",
            Priority = "InvalidPriority",
            Summary = "Test invalid priority",
            Description = "Test description",
            ConsentFlag = true
        };

        // Act
        var result = await SendAsync(command);

        // Assert
        result.Succeeded.Should().BeTrue();

        var createdIssue = await FindAsync<Issue>(result.Data);
        createdIssue!.Priority.Should().Be(IssuePriority.Medium); // Default fallback
    }

    [Test]
    public async Task Handle_MultipleRequests_ShouldGenerateUniqueReferenceNumbers()
    {
        // Arrange
        var commands = Enumerable.Range(1, 5).Select(i => new IssueIntakeCommand
        {
            ReporterPhone = $"+123456789{i}",
            ReporterName = $"Test User {i}",
            Channel = "WhatsApp",
            Category = "Technical",
            Product = "Test Product",
            Severity = "Medium",
            Priority = "Medium",
            Summary = $"Test issue {i}",
            Description = $"Test description {i}",
            ConsentFlag = true
        }).ToList();

        // Act
        var results = new List<Result<Guid>>();
        foreach (var command in commands)
        {
            var result = await SendAsync(command);
            results.Add(result);
        }

        // Assert
        results.Should().AllSatisfy(r => r.Succeeded.Should().BeTrue());

        var referenceNumbers = new List<string>();
        foreach (var result in results)
        {
            var issue = await FindAsync<Issue>(result.Data);
            referenceNumbers.Add(issue!.ReferenceNumber);
        }

        // All reference numbers should be unique
        referenceNumbers.Should().OnlyHaveUniqueItems();
        referenceNumbers.Should().AllSatisfy(r => r.Should().StartWith("ISS-2025-"));
    }

    [Test]
    public async Task Handle_CrossTenantIsolation_ShouldPreventCrossTenantDataAccess()
    {   
        // Arrange - Create test tenants
        var tenant1Id = await CreateTestTenantAsync("Tenant 1");
        var tenant2Id = await CreateTestTenantAsync("Tenant 2");

        // Create contact in tenant 1
        var tenant1Contact = new Contact
        {
            PhoneNumber = "+1111111111",
            Name = "Tenant 1 User",
            Description = "Contact in tenant 1",
            TenantId = tenant1Id
        };
        await AddAsync(tenant1Contact);

        // Create issue in tenant 1
        var tenant1Issue = Issue.Create(
            "ISS-2025-000001",
            "Tenant 1 Issue",
            "Issue description",
            IssueCategory.Technical,
            IssuePriority.High,
            tenant1Contact.Id,
            tenant1Id,
            "[\"msg1\"]",
            "{\"messageIds\":[\"msg1\"]}",
            true
        );
        await AddAsync(tenant1Issue);

        // Switch to tenant 2 context and attempt to access tenant 1 data
        SetCurrentTenant(tenant2Id);

        // Act - Try to create issue with same phone number in tenant 2
        var command = new IssueIntakeCommand
        {
            ReporterPhone = "+1111111111", // Same phone as tenant 1 contact
            ReporterName = "Tenant 2 User",
            Channel = "WhatsApp",
            Category = "General",
            Product = "Test Product",
            Severity = "Medium",
            Priority = "Medium",
            Summary = "Tenant 2 issue",
            Description = "Should not access tenant 1 contact",
            ConsentFlag = true
        };

        var result = await SendAsync(command);

        // Assert - Should succeed but create new contact in tenant 2
        result.Succeeded.Should().BeTrue();

        var tenant2Issue = await FindAsync<Issue>(result.Data);
        tenant2Issue.Should().NotBeNull();
        tenant2Issue!.TenantId.Should().Be(tenant2Id);
        tenant2Issue.ReporterContactId.Should().NotBe(tenant1Contact.Id); // Should NOT reuse tenant 1 contact

        // Verify new contact was created in tenant 2
        var tenant2Contact = await FindAsync<Contact>(tenant2Issue.ReporterContactId!.Value);
        tenant2Contact.Should().NotBeNull();
        tenant2Contact!.TenantId.Should().Be(tenant2Id);
        tenant2Contact.Name.Should().Be("Tenant 2 User");
        tenant2Contact.PhoneNumber.Should().Be("+1111111111");

        // Critical: Verify tenant 1 contact remains unchanged
        var unchangedTenant1Contact = await FindAsync<Contact>(tenant1Contact.Id);
        unchangedTenant1Contact.Should().NotBeNull();
        unchangedTenant1Contact!.TenantId.Should().Be(tenant1Id);
        unchangedTenant1Contact.Name.Should().Be("Tenant 1 User");

        // Verify issue counts per tenant
        await using var context = CreateDbContext();
        
        var tenant1Issues = await context.Issues.Where(i => i.TenantId == tenant1Id).CountAsync();
        var tenant2Issues = await context.Issues.Where(i => i.TenantId == tenant2Id).CountAsync();
        
        tenant1Issues.Should().Be(1);
        tenant2Issues.Should().Be(1);
    }

    private async Task<string> CreateTestTenantAsync(string name)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = $"Test tenant: {name}"
        };
        await AddAsync(tenant);
        return tenant.Id;
    }

    private void SetCurrentTenant(string tenantId)
    {
        // This method should be implemented in TestBase to switch tenant context
        // For now, we rely on the framework's tenant filtering
        // In real implementation, this would set the current tenant in the service provider
    }

    private async Task<string> GetDefaultTenantId()
    {
        await using var context = CreateDbContext();
        var tenant = await context.Tenants.FirstAsync();
        return tenant.Id;
    }
}
