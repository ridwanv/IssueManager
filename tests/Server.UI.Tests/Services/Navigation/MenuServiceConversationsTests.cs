using CleanArchitecture.Blazor.Server.UI.Services.Navigation;
using FluentAssertions;
using Xunit;

namespace CleanArchitecture.Blazor.Server.UI.Tests.Services.Navigation;

public class MenuServiceConversationsTests
{
    private readonly MenuService _menuService;

    public MenuServiceConversationsTests()
    {
        _menuService = new MenuService();
    }

    [Fact]
    public void MenuService_Should_Have_Conversations_Section()
    {
        // Act
        var features = _menuService.Features;
        var conversationsSection = features.FirstOrDefault(f => f.Title == "CONVERSATIONS");

        // Assert
        conversationsSection.Should().NotBeNull();
        conversationsSection!.SectionItems.Should().HaveCount(3);
    }

    [Fact]
    public void Conversations_Section_Should_Have_My_Conversations_Item()
    {
        // Act
        var features = _menuService.Features;
        var conversationsSection = features.First(f => f.Title == "CONVERSATIONS");
        var myConversationsItem = conversationsSection.SectionItems.FirstOrDefault(i => i.Title == "My Conversations");

        // Assert
        myConversationsItem.Should().NotBeNull();
        myConversationsItem!.Href.Should().Be("/agent/my-conversations");
        myConversationsItem.Icon.Should().Be(Icons.Material.Filled.PersonalVideo);
        myConversationsItem.PageStatus.Should().Be(PageStatus.Completed);
    }

    [Fact]
    public void Conversations_Section_Should_Have_Pending_Conversations_Item()
    {
        // Act
        var features = _menuService.Features;
        var conversationsSection = features.First(f => f.Title == "CONVERSATIONS");
        var pendingConversationsItem = conversationsSection.SectionItems.FirstOrDefault(i => i.Title == "Pending Conversations");

        // Assert
        pendingConversationsItem.Should().NotBeNull();
        pendingConversationsItem!.Href.Should().Be("/agent/pending-conversations");
        pendingConversationsItem.Icon.Should().Be(Icons.Material.Filled.PendingActions);
        pendingConversationsItem.PageStatus.Should().Be(PageStatus.Completed);
    }

    [Fact]
    public void Conversations_Section_Should_Have_Conversation_Manager_Item()
    {
        // Act
        var features = _menuService.Features;
        var conversationsSection = features.First(f => f.Title == "CONVERSATIONS");
        var conversationManagerItem = conversationsSection.SectionItems.FirstOrDefault(i => i.Title == "Conversation Manager");

        // Assert
        conversationManagerItem.Should().NotBeNull();
        conversationManagerItem!.Href.Should().Be("/agent-dashboard");
        conversationManagerItem.Icon.Should().Be(Icons.Material.Filled.Dashboard);
        conversationManagerItem.PageStatus.Should().Be(PageStatus.Completed);
    }

    [Fact]
    public void Agent_Section_Should_Still_Exist_With_Agent_Tools()
    {
        // Act
        var features = _menuService.Features;
        var agentSection = features.FirstOrDefault(f => f.Title == "AGENT");

        // Assert
        agentSection.Should().NotBeNull();
        agentSection!.SectionItems.Should().HaveCountGreaterOrEqualTo(1);
        
        var agentToolsItem = agentSection.SectionItems.FirstOrDefault(i => i.Title == "Agent Tools");
        agentToolsItem.Should().NotBeNull();
    }
}
