using CleanArchitecture.Blazor.Application.Common.Constants.Roles;
using CleanArchitecture.Blazor.Server.UI.Models.NavigationMenu;
using CleanArchitecture.Blazor.Server.UI.Services.Navigation;
using FluentAssertions;
using Xunit;

namespace CleanArchitecture.Blazor.Server.UI.Tests.Services.Navigation;

public class MenuServiceIssuesTests
{
    private readonly MenuService _menuService;

    public MenuServiceIssuesTests()
    {
        _menuService = new MenuService();
    }

    [Fact]
    public void MenuService_Should_Have_Issues_Section()
    {
        // Act
        var features = _menuService.Features;
        var issuesSection = features.FirstOrDefault(f => f.Title == "ISSUES");

        // Assert
        issuesSection.Should().NotBeNull();
        issuesSection!.SectionItems.Should().HaveCount(3);
    }

    [Fact]
    public void Issues_Section_Should_Have_My_Issues_Item()
    {
        // Act
        var features = _menuService.Features;
        var issuesSection = features.First(f => f.Title == "ISSUES");
        var myIssuesItem = issuesSection.SectionItems.FirstOrDefault(i => i.Title == "My Issues");

        // Assert
        myIssuesItem.Should().NotBeNull();
        myIssuesItem!.Href.Should().Be("/my-issues");
        myIssuesItem.Icon.Should().Be(Icons.Material.Filled.AssignmentInd);
        myIssuesItem.PageStatus.Should().Be(PageStatus.Completed);
        myIssuesItem.Roles.Should().BeNull(); // Available to all authenticated users
    }

    [Fact]
    public void Issues_Section_Should_Have_Issue_Management_Item()
    {
        // Act
        var features = _menuService.Features;
        var issuesSection = features.First(f => f.Title == "ISSUES");
        var issueManagementItem = issuesSection.SectionItems.FirstOrDefault(i => i.Title == "Issue Management");

        // Assert
        issueManagementItem.Should().NotBeNull();
        issueManagementItem!.Href.Should().Be("/issues");
        issueManagementItem.Icon.Should().Be(Icons.Material.Filled.List);
        issueManagementItem.PageStatus.Should().Be(PageStatus.Completed);
        issueManagementItem.Roles.Should().BeNull(); // Available to all authenticated users
    }

    [Fact]
    public void Issues_Section_Should_Have_Issue_Analytics_Item_With_Role_Restrictions()
    {
        // Act
        var features = _menuService.Features;
        var issuesSection = features.First(f => f.Title == "ISSUES");
        var issueAnalyticsItem = issuesSection.SectionItems.FirstOrDefault(i => i.Title == "Issue Analytics");

        // Assert
        issueAnalyticsItem.Should().NotBeNull();
        issueAnalyticsItem!.Href.Should().Be("/issues/analytics");
        issueAnalyticsItem.Icon.Should().Be(Icons.Material.Filled.Analytics);
        issueAnalyticsItem.PageStatus.Should().Be(PageStatus.Completed);
        issueAnalyticsItem.Roles.Should().NotBeNull();
        issueAnalyticsItem.Roles.Should().Contain(RoleName.Admin);
        issueAnalyticsItem.Roles.Should().Contain(RoleName.Users);
        issueAnalyticsItem.Roles.Should().HaveCount(2);
    }

    [Fact]
    public void Application_Section_Should_Not_Have_Issue_Related_Items()
    {
        // Act
        var features = _menuService.Features;
        var applicationSection = features.First(f => f.Title == "Application");

        // Assert - Application section should no longer contain issue-related items
        var issuesItem = applicationSection.SectionItems.FirstOrDefault(i => i.Title == "Issues");
        var myIssuesItem = applicationSection.SectionItems.FirstOrDefault(i => i.Title == "My Issues");
        var issueAnalyticsItem = applicationSection.SectionItems.FirstOrDefault(i => i.Title == "Issue Analytics");

        issuesItem.Should().BeNull();
        myIssuesItem.Should().BeNull();
        issueAnalyticsItem.Should().BeNull();
    }

    [Fact]
    public void Application_Section_Should_Still_Have_Home_And_ECommerce()
    {
        // Act
        var features = _menuService.Features;
        var applicationSection = features.First(f => f.Title == "Application");

        // Assert - Application section should still have Home and E-Commerce
        var homeItem = applicationSection.SectionItems.FirstOrDefault(i => i.Title == "Home");
        var ecommerceItem = applicationSection.SectionItems.FirstOrDefault(i => i.Title == "E-Commerce");

        homeItem.Should().NotBeNull();
        homeItem!.Href.Should().Be("/");
        
        ecommerceItem.Should().NotBeNull();
        ecommerceItem!.IsParent.Should().BeTrue();
        ecommerceItem.MenuItems.Should().NotBeNull();
        ecommerceItem.MenuItems.Should().HaveCount(3);
    }

    [Fact]
    public void Menu_Sections_Should_Be_In_Correct_Order()
    {
        // Act
        var features = _menuService.Features.ToList();

        // Assert - Verify the ordering: Application, ISSUES, CONVERSATIONS, AGENT, MANAGEMENT
        features[0].Title.Should().Be("Application");
        features[1].Title.Should().Be("ISSUES");
        features[2].Title.Should().Be("CONVERSATIONS");
        features[3].Title.Should().Be("AGENT");
        features[4].Title.Should().Be("MANAGEMENT");
    }

    [Fact]
    public void Issues_Section_Items_Should_Be_In_Correct_Order()
    {
        // Act
        var features = _menuService.Features;
        var issuesSection = features.First(f => f.Title == "ISSUES");
        var items = issuesSection.SectionItems.ToList();

        // Assert - Verify the ordering: My Issues, Issue Management, Issue Analytics
        items[0].Title.Should().Be("My Issues");
        items[1].Title.Should().Be("Issue Management");
        items[2].Title.Should().Be("Issue Analytics");
    }
}
