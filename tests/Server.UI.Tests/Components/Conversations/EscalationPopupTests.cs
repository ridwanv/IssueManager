using AngleSharp;
using Bunit;
using FluentAssertions;
using IssueManager.Application.Features.Conversations.DTOs;
using IssueManager.Application.Features.Conversations.Queries.GetConversationContext;
using IssueManager.Application.Features.Conversations.Commands.AcceptEscalation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using Xunit;
using IssueManager.Server.UI.Components.Conversations;
using Microsoft.AspNetCore.Components;

namespace IssueManager.Server.UI.Tests.Components.Conversations;

public class EscalationPopupTests : TestContext
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IJSRuntime> _mockJSRuntime;
    private readonly Mock<NavigationManager> _mockNavigation;

    public EscalationPopupTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockJSRuntime = new Mock<IJSRuntime>();
        _mockNavigation = new Mock<NavigationManager>();

        Services.AddScoped(_ => _mockMediator.Object);
        Services.AddScoped(_ => _mockJSRuntime.Object);
        Services.AddScoped(_ => _mockNavigation.Object);
        Services.AddMudServices();
    }

    [Fact]
    public void EscalationPopup_InitiallyHidden_DoesNotRenderContent()
    {
        // Arrange & Act
        var component = RenderComponent<EscalationPopup>();

        // Assert
        var overlay = component.FindAll(".escalation-popup-overlay");
        overlay.Should().BeEmpty("popup should be hidden initially");
    }

    [Fact]
    public async Task ShowEscalation_ValidData_DisplaysPopup()
    {
        // Arrange
        var escalationData = new EscalationPopupDto
        {
            ConversationId = 123,
            CustomerName = "John Doe",
            PhoneNumber = "+1234567890",
            EscalationReason = "Billing issue",
            Priority = 2,
            EscalatedAt = DateTime.UtcNow.AddMinutes(-5),
            MessageCount = 3,
            ConversationDuration = TimeSpan.FromMinutes(15),
            LastMessage = "I need help with my bill"
        };

        _mockMediator.Setup(m => m.Send(It.IsAny<GetConversationContextQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EscalationPopupDto>.Success(escalationData));

        var component = RenderComponent<EscalationPopup>();

        // Act
        await component.Instance.ShowEscalation(123);

        // Assert
        var overlay = component.Find(".escalation-popup-overlay");
        overlay.Should().NotBeNull("popup should be visible");

        var customerInfo = component.Find("strong:contains('Name:')").Parent;
        customerInfo.TextContent.Should().Contain("John Doe");

        var phoneInfo = component.Find("strong:contains('Phone:')").Parent;
        phoneInfo.TextContent.Should().Contain("+1234567890");

        var reasonInfo = component.Find("strong:contains('Reason:')").Parent;
        reasonInfo.TextContent.Should().Contain("Billing issue");

        var priorityChip = component.Find(".mud-chip:contains('HIGH')");
        priorityChip.Should().NotBeNull("priority should be displayed for high priority escalations");
    }

    [Fact]
    public async Task ShowEscalation_CriticalPriority_ShowsCriticalChip()
    {
        // Arrange
        var escalationData = new EscalationPopupDto
        {
            ConversationId = 123,
            CustomerName = "Jane Smith",
            PhoneNumber = "+1234567890",
            EscalationReason = "System down",
            Priority = 3, // Critical
            EscalatedAt = DateTime.UtcNow.AddMinutes(-2),
            MessageCount = 1,
            ConversationDuration = TimeSpan.FromMinutes(2)
        };

        _mockMediator.Setup(m => m.Send(It.IsAny<GetConversationContextQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EscalationPopupDto>.Success(escalationData));

        var component = RenderComponent<EscalationPopup>();

        // Act
        await component.Instance.ShowEscalation(123);

        // Assert
        var criticalChip = component.Find(".mud-chip:contains('CRITICAL')");
        criticalChip.Should().NotBeNull("critical priority should be displayed");
        
        // Check that the chip has error color class
        criticalChip.ClassList.Should().Contain("mud-chip-color-error");
    }

    [Fact]
    public async Task AcceptButton_Clicked_CallsAcceptEscalationCommand()
    {
        // Arrange
        var escalationData = new EscalationPopupDto
        {
            ConversationId = 456,
            CustomerName = "Bob Wilson",
            PhoneNumber = "+1234567890",
            EscalationReason = "Technical issue",
            Priority = 1,
            EscalatedAt = DateTime.UtcNow.AddMinutes(-3),
            MessageCount = 2,
            ConversationDuration = TimeSpan.FromMinutes(10)
        };

        _mockMediator.Setup(m => m.Send(It.IsAny<GetConversationContextQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EscalationPopupDto>.Success(escalationData));

        _mockMediator.Setup(m => m.Send(It.IsAny<AcceptEscalationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Success());

        var component = RenderComponent<EscalationPopup>();
        await component.Instance.ShowEscalation(456);

        // Act
        var acceptButton = component.Find("button:contains('Accept & View')");
        await acceptButton.ClickAsync();

        // Assert
        _mockMediator.Verify(m => m.Send(
            It.Is<AcceptEscalationCommand>(cmd => cmd.ConversationId == 456),
            It.IsAny<CancellationToken>()), Times.Once);
        
        // Verify navigation was called
        _mockNavigation.Verify(n => n.NavigateTo("/agent/conversations/456"), Times.Once);
    }

    [Fact]
    public async Task HidePopup_Called_HidesPopupAndClearsData()
    {
        // Arrange
        var escalationData = new EscalationPopupDto
        {
            ConversationId = 789,
            CustomerName = "Alice Johnson",
            PhoneNumber = "+1234567890",
            EscalationReason = "Account locked",
            Priority = 2,
            EscalatedAt = DateTime.UtcNow.AddMinutes(-1),
            MessageCount = 1,
            ConversationDuration = TimeSpan.FromMinutes(1)
        };

        _mockMediator.Setup(m => m.Send(It.IsAny<GetConversationContextQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EscalationPopupDto>.Success(escalationData));

        var component = RenderComponent<EscalationPopup>();
        await component.Instance.ShowEscalation(789);

        // Verify popup is shown
        var overlay = component.Find(".escalation-popup-overlay");
        overlay.Should().NotBeNull();

        // Act
        component.Instance.HidePopup();

        // Assert
        var hiddenOverlays = component.FindAll(".escalation-popup-overlay");
        hiddenOverlays.Should().BeEmpty("popup should be hidden after calling HidePopup");
    }

    [Fact]
    public async Task ShowEscalation_ServiceFailure_DoesNotShowPopup()
    {
        // Arrange
        _mockMediator.Setup(m => m.Send(It.IsAny<GetConversationContextQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EscalationPopupDto>.Failure("Conversation not found"));

        var component = RenderComponent<EscalationPopup>();

        // Act
        await component.Instance.ShowEscalation(999);

        // Assert
        var overlays = component.FindAll(".escalation-popup-overlay");
        overlays.Should().BeEmpty("popup should not be shown when service fails");
    }

    [Fact]
    public void FormatDuration_VariousTimeSpans_FormatsCorrectly()
    {
        // This test would need to be adjusted based on the actual implementation
        // since the FormatDuration method is private. We can test it indirectly
        // through the displayed content.

        // Arrange
        var escalationData = new EscalationPopupDto
        {
            ConversationId = 100,
            CustomerName = "Test User",
            PhoneNumber = "+1234567890",
            EscalationReason = "Test",
            Priority = 1,
            EscalatedAt = DateTime.UtcNow.AddHours(-2),
            MessageCount = 5,
            ConversationDuration = TimeSpan.FromMinutes(90) // 1h 30m
        };

        _mockMediator.Setup(m => m.Send(It.IsAny<GetConversationContextQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EscalationPopupDto>.Success(escalationData));

        var component = RenderComponent<EscalationPopup>();

        // Act
        component.Instance.ShowEscalation(100).Wait();

        // Assert
        var durationText = component.Find("strong:contains('Duration:')").Parent;
        durationText.TextContent.Should().Contain("1h 30m", "duration should be formatted as hours and minutes");
    }
}