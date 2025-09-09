using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using CleanArchitecture.Blazor.Application.Features.Contacts.DTOs;
using CleanArchitecture.Blazor.Server.UI.Components.Issues;
using MudBlazor;

namespace CleanArchitecture.Blazor.Server.UI.Tests.Components.Issues;

public class ReporterContactCardTests : TestContext
{
    public ReporterContactCardTests()
    {
        Services.AddMudServices();
    }

    [Fact]
    public void ReporterContactCard_ShouldRenderWithCompleteContactInformation()
    {
        // Arrange
        var contact = new ContactDto
        {
            Id = 1,
            Name = "John Doe",
            PhoneNumber = "+1234567890",
            Email = "john.doe@example.com",
            Country = "United States",
            Description = "Primary contact for test cases"
        };

        // Act
        var component = RenderComponent<ReporterContactCard>(parameters => parameters
            .Add(p => p.Contact, contact));

        // Assert
        Assert.Contains("Reporter Information", component.Markup);
        Assert.Contains("John Doe", component.Markup);
        Assert.Contains("+1234567890", component.Markup);
        Assert.Contains("john.doe@example.com", component.Markup);
        Assert.Contains("United States", component.Markup);
        Assert.Contains("Primary contact for test cases", component.Markup);
        
        // Verify icons and structure
        Assert.Contains("Icons.Material.Filled.Person", component.Markup);
        Assert.Contains("Icons.Material.Filled.WhatsApp", component.Markup);
        Assert.Contains("Icons.Material.Filled.Email", component.Markup);
        Assert.Contains("Icons.Material.Filled.LocationOn", component.Markup);
        Assert.Contains("Icons.Material.Filled.Notes", component.Markup);
        
        // Verify primary contact indicator
        Assert.Contains("Primary", component.Markup);
    }

    [Fact]
    public void ReporterContactCard_ShouldHandlePartialContactInformation()
    {
        // Arrange
        var contact = new ContactDto
        {
            Id = 1,
            Name = "Jane Smith",
            PhoneNumber = "+0987654321"
            // Email, Country, and Description are null/empty
        };

        // Act
        var component = RenderComponent<ReporterContactCard>(parameters => parameters
            .Add(p => p.Contact, contact));

        // Assert
        Assert.Contains("Reporter Information", component.Markup);
        Assert.Contains("Jane Smith", component.Markup);
        Assert.Contains("+0987654321", component.Markup);
        
        // These should not be present since they're null/empty
        Assert.DoesNotContain("Icons.Material.Filled.Email", component.Markup);
        Assert.DoesNotContain("Icons.Material.Filled.LocationOn", component.Markup);
        Assert.DoesNotContain("Icons.Material.Filled.Notes", component.Markup);
    }

    [Fact]
    public void ReporterContactCard_ShouldHandleContactWithoutPhoneOrEmail()
    {
        // Arrange
        var contact = new ContactDto
        {
            Id = 1,
            Name = "No Contact Person",
            Country = "Unknown"
            // PhoneNumber and Email are null/empty
        };

        // Act
        var component = RenderComponent<ReporterContactCard>(parameters => parameters
            .Add(p => p.Contact, contact));

        // Assert
        Assert.Contains("Reporter Information", component.Markup);
        Assert.Contains("No Contact Person", component.Markup);
        Assert.Contains("Unknown", component.Markup);
        
        // Should show fallback message for missing contact methods
        Assert.Contains("No contact methods available", component.Markup);
        
        // Should not show WhatsApp or Email sections
        Assert.DoesNotContain("Icons.Material.Filled.WhatsApp", component.Markup);
        Assert.DoesNotContain("Icons.Material.Filled.Email", component.Markup);
    }

    [Fact]
    public void ReporterContactCard_ShouldHandleNullContact()
    {
        // Act
        var component = RenderComponent<ReporterContactCard>(parameters => parameters
            .Add(p => p.Contact, (ContactDto?)null));

        // Assert
        Assert.Contains("Reporter Information", component.Markup);
        Assert.Contains("Reporter information is not available", component.Markup);
        
        // Should show warning alert
        Assert.Contains("Icons.Material.Filled.Warning", component.Markup);
    }

    [Fact]
    public void ReporterContactCard_ShouldRenderResponsiveLayout()
    {
        // Arrange
        var contact = new ContactDto
        {
            Id = 1,
            Name = "Test User",
            PhoneNumber = "+1234567890",
            Email = "test@example.com"
        };

        // Act
        var component = RenderComponent<ReporterContactCard>(parameters => parameters
            .Add(p => p.Contact, contact));

        // Assert - Check for responsive grid classes
        Assert.Contains("xs=\"12\"", component.Markup);
        Assert.Contains("md=\"6\"", component.Markup);
        
        // Verify card structure for mobile responsiveness
        Assert.Contains("MudCard", component.Markup);
        Assert.Contains("MudGrid", component.Markup);
        Assert.Contains("MudItem", component.Markup);
    }

    [Fact]
    public void ReporterContactCard_ShouldDisplayCorrectContactMethodIcons()
    {
        // Arrange
        var contact = new ContactDto
        {
            Id = 1,
            Name = "Icon Test User",
            PhoneNumber = "+1111111111",
            Email = "icon@test.com",
            Country = "Test Country",
            Description = "Testing icons"
        };

        // Act
        var component = RenderComponent<ReporterContactCard>(parameters => parameters
            .Add(p => p.Contact, contact));

        // Assert - Verify all expected icons are present
        var markup = component.Markup;
        Assert.Contains("Icons.Material.Filled.Person", markup); // Header icon
        Assert.Contains("Icons.Material.Filled.AccountCircle", markup); // Name icon
        Assert.Contains("Icons.Material.Filled.WhatsApp", markup); // Phone icon
        Assert.Contains("Icons.Material.Filled.Email", markup); // Email icon
        Assert.Contains("Icons.Material.Filled.LocationOn", markup); // Country icon
        Assert.Contains("Icons.Material.Filled.Notes", markup); // Description icon
    }

    [Fact]
    public void ReporterContactCard_ShouldShowPrimaryContactIndicator()
    {
        // Arrange
        var contact = new ContactDto
        {
            Id = 1,
            Name = "Primary Contact Test",
            PhoneNumber = "+1111111111"
        };

        // Act
        var component = RenderComponent<ReporterContactCard>(parameters => parameters
            .Add(p => p.Contact, contact));

        // Assert
        Assert.Contains("Primary", component.Markup);
        Assert.Contains("WhatsApp", component.Markup);
        
        // Verify the chip styling
        Assert.Contains("MudChip", component.Markup);
        Assert.Contains("Color.Success", component.Markup);
    }
}
