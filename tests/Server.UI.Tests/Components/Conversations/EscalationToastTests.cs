using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Bunit;
using Bunit.TestDoubles;
using MudBlazor.Services;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;
using CleanArchitecture.Blazor.Server.UI.Components.Conversations;
using MediatR;
using Microsoft.JSInterop;
using MudBlazor;
using Microsoft.Extensions.Logging;
using AngleSharp.Dom;
using System;
using System.Threading.Tasks;
using Moq;

namespace IssueManager.Server.UI.Tests.Components.Conversations
{
    public class EscalationToastTests : TestContext
    {
        public EscalationToastTests()
        {
            // Register required services
            Services.AddMudServices();
            Services.AddScoped<IMediator>(_ => Mock.Of<IMediator>());
            Services.AddScoped<ISnackbar>(_ => Mock.Of<ISnackbar>());
            
            // Mock IJSRuntime
            Services.AddScoped<IJSRuntime>(_ => 
            {
                var mock = new Mock<IJSRuntime>();
                mock.Setup(x => x.InvokeAsync<object>(It.IsAny<string>(), It.IsAny<object[]>()))
                    .Returns(new ValueTask<object>());
                return mock.Object;
            });
            
            // Mock NavigationManager
            Services.AddSingleton<FakeNavigationManager>();
        }

        [Fact]
        public void EscalationToast_WhenNotVisible_ShouldNotRender()
        {
            // Arrange & Act
            var component = RenderComponent<EscalationToast>(parameters => parameters
                .Add(p => p.IsVisible, false)
                .Add(p => p.EscalationData, CreateSampleEscalationData()));

            // Assert
            Assert.Empty(component.FindAll(".escalation-toast"));
        }

        [Fact]
        public void EscalationToast_WhenVisible_ShouldRender()
        {
            // Arrange
            var escalationData = CreateSampleEscalationData();

            // Act
            var component = RenderComponent<EscalationToast>(parameters => parameters
                .Add(p => p.IsVisible, true)
                .Add(p => p.EscalationData, escalationData));

            // Assert
            var toastElement = component.Find(".escalation-toast");
            Assert.NotNull(toastElement);
            
            // Check customer information is displayed
            Assert.Contains(escalationData.CustomerName, component.Markup);
            Assert.Contains(escalationData.PhoneNumber, component.Markup);
            Assert.Contains(escalationData.EscalationReason, component.Markup);
        }

        [Fact]
        public void EscalationToast_WhenCriticalPriority_ShouldShowCriticalStyling()
        {
            // Arrange
            var criticalEscalation = CreateSampleEscalationData();
            criticalEscalation.Priority = 3; // Critical priority

            // Act
            var component = RenderComponent<EscalationToast>(parameters => parameters
                .Add(p => p.IsVisible, true)
                .Add(p => p.EscalationData, criticalEscalation));

            // Assert
            var toastElement = component.Find(".escalation-toast-critical");
            Assert.NotNull(toastElement);
            
            // Check for critical chip
            Assert.Contains("CRITICAL", component.Markup);
        }

        [Fact]
        public void EscalationToast_WhenStandardPriority_ShouldShowStandardStyling()
        {
            // Arrange
            var standardEscalation = CreateSampleEscalationData();
            standardEscalation.Priority = 1; // Standard priority

            // Act
            var component = RenderComponent<EscalationToast>(parameters => parameters
                .Add(p => p.IsVisible, true)
                .Add(p => p.EscalationData, standardEscalation));

            // Assert
            var toastElement = component.Find(".escalation-toast-standard");
            Assert.NotNull(toastElement);
            
            // Should not contain critical chip
            Assert.DoesNotContain("CRITICAL", component.Markup);
        }

        [Fact]
        public void EscalationToast_ShouldContainAllActionButtons()
        {
            // Arrange
            var escalationData = CreateSampleEscalationData();

            // Act
            var component = RenderComponent<EscalationToast>(parameters => parameters
                .Add(p => p.IsVisible, true)
                .Add(p => p.EscalationData, escalationData));

            // Assert
            var buttons = component.FindAll("button");
            
            // Should have Dismiss, Decline, Accept, and Close buttons
            Assert.True(buttons.Count >= 4);
            
            // Check button text content
            var buttonTexts = string.Join(" ", buttons.Select(b => b.TextContent));
            Assert.Contains("Dismiss", buttonTexts);
            Assert.Contains("Decline", buttonTexts);
            Assert.Contains("Accept", buttonTexts);
        }

        [Fact]
        public async Task EscalationToast_DismissButton_ShouldTriggerOnDismissedCallback()
        {
            // Arrange
            var escalationData = CreateSampleEscalationData();
            var callbackTriggered = false;
            var triggeredConversationId = string.Empty;

            // Act
            var component = RenderComponent<EscalationToast>(parameters => parameters
                .Add(p => p.IsVisible, true)
                .Add(p => p.EscalationData, escalationData)
                .Add(p => p.OnDismissed, EventCallback.Factory.Create<string>(this, (conversationId) =>
                {
                    callbackTriggered = true;
                    triggeredConversationId = conversationId;
                })));

            // Find and click dismiss button
            var dismissButton = component.FindAll("button")
                .FirstOrDefault(b => b.TextContent.Contains("Dismiss"));
            Assert.NotNull(dismissButton);

            await dismissButton.ClickAsync();

            // Assert
            Assert.True(callbackTriggered);
            Assert.Equal(escalationData.ConversationReference, triggeredConversationId);
        }

        [Fact]
        public void EscalationToast_ShouldShowSwipeIndicator()
        {
            // Arrange
            var escalationData = CreateSampleEscalationData();

            // Act
            var component = RenderComponent<EscalationToast>(parameters => parameters
                .Add(p => p.IsVisible, true)
                .Add(p => p.EscalationData, escalationData));

            // Assert
            var swipeIndicator = component.Find(".escalation-toast-swipe-indicator");
            Assert.NotNull(swipeIndicator);
        }

        [Fact]
        public void EscalationToast_ShouldTruncateLongEscalationReason()
        {
            // Arrange
            var escalationData = CreateSampleEscalationData();
            escalationData.EscalationReason = new string('A', 100); // Very long reason

            // Act
            var component = RenderComponent<EscalationToast>(parameters => parameters
                .Add(p => p.IsVisible, true)
                .Add(p => p.EscalationData, escalationData));

            // Assert
            // The component should show truncated text with ellipsis
            Assert.Contains("...", component.Markup);
        }

        [Fact]
        public void EscalationToast_ShouldShowTimeAgo()
        {
            // Arrange
            var escalationData = CreateSampleEscalationData();
            escalationData.EscalatedAt = DateTime.UtcNow.AddMinutes(-5); // 5 minutes ago

            // Act
            var component = RenderComponent<EscalationToast>(parameters => parameters
                .Add(p => p.IsVisible, true)
                .Add(p => p.EscalationData, escalationData));

            // Assert
            Assert.Contains("5m ago", component.Markup);
        }

        private static EscalationPopupDto CreateSampleEscalationData()
        {
            return new EscalationPopupDto
            {
                ConversationReference = "test-conversation-123",
                CustomerName = "John Doe",
                PhoneNumber = "+1234567890",
                EscalationReason = "Customer needs urgent assistance with billing issue",
                Priority = 2,
                EscalatedAt = DateTime.UtcNow.AddMinutes(-2),
                MessageCount = 15,
                ConversationDuration = TimeSpan.FromMinutes(10),
                LastMessage = "I really need help with this billing problem"
            };
        }
    }
}
