using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Bunit;
using Bunit.TestDoubles;
using MudBlazor.Services;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;
using CleanArchitecture.Blazor.Server.UI.Components.Conversations;
using CleanArchitecture.Blazor.Server.UI.Services.SignalR;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Moq;

namespace IssueManager.Server.UI.Tests.Components.Conversations
{
    public class EscalationToastContainerTests : TestContext
    {
        private readonly Mock<SignalRConnectionService> _mockSignalRService;
        private readonly Mock<IJSRuntime> _mockJSRuntime;
        private readonly Mock<ILogger<EscalationToastContainer>> _mockLogger;

        public EscalationToastContainerTests()
        {
            // Setup mocks
            _mockSignalRService = new Mock<SignalRConnectionService>();
            _mockJSRuntime = new Mock<IJSRuntime>();
            _mockLogger = new Mock<ILogger<EscalationToastContainer>>();

            // Register required services
            Services.AddMudServices();
            Services.AddScoped(_ => _mockSignalRService.Object);
            Services.AddScoped(_ => _mockJSRuntime.Object);
            Services.AddScoped(_ => _mockLogger.Object);
            
            // Mock IJSRuntime for sound notifications
            _mockJSRuntime.Setup(x => x.InvokeVoidAsync("playNotificationSound", It.IsAny<object[]>()))
                .Returns(ValueTask.CompletedTask);
        }

        [Fact]
        public void EscalationToastContainer_InitialState_ShouldBeEmpty()
        {
            // Act
            var component = RenderComponent<EscalationToastContainer>();

            // Assert
            var toastWrappers = component.FindAll(".escalation-toast-wrapper");
            Assert.Empty(toastWrappers);
            Assert.Equal(0, component.Instance.GetActiveToastCount());
            Assert.False(component.Instance.HasActiveToasts());
        }

        [Fact]
        public async Task EscalationToastContainer_ShowToast_ShouldAddToast()
        {
            // Arrange
            var component = RenderComponent<EscalationToastContainer>();
            var escalationData = CreateSampleEscalationData();

            // Act
            await component.Instance.ShowToast(escalationData);

            // Assert
            Assert.Equal(1, component.Instance.GetActiveToastCount());
            Assert.True(component.Instance.HasActiveToasts());
            
            // Verify JSRuntime was called for sound notification
            _mockJSRuntime.Verify(x => x.InvokeVoidAsync("playNotificationSound", 
                It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == "escalation")), 
                Times.Once);
        }

        [Fact]
        public async Task EscalationToastContainer_ShowMultipleToasts_ShouldStackCorrectly()
        {
            // Arrange
            var component = RenderComponent<EscalationToastContainer>(parameters => parameters
                .Add(p => p.MaxToastCount, 3));

            var escalation1 = CreateSampleEscalationData();
            escalation1.ConversationReference = "conv-1";
            
            var escalation2 = CreateSampleEscalationData();
            escalation2.ConversationReference = "conv-2";

            // Act
            await component.Instance.ShowToast(escalation1);
            await component.Instance.ShowToast(escalation2);

            // Assert
            Assert.Equal(2, component.Instance.GetActiveToastCount());
            Assert.True(component.Instance.HasActiveToasts());
        }

        [Fact]
        public async Task EscalationToastContainer_ExceedMaxToastCount_ShouldRemoveOldest()
        {
            // Arrange
            var component = RenderComponent<EscalationToastContainer>(parameters => parameters
                .Add(p => p.MaxToastCount, 2));

            var escalation1 = CreateSampleEscalationData();
            escalation1.ConversationReference = "conv-1";
            
            var escalation2 = CreateSampleEscalationData();
            escalation2.ConversationReference = "conv-2";
            
            var escalation3 = CreateSampleEscalationData();
            escalation3.ConversationReference = "conv-3";

            // Act
            await component.Instance.ShowToast(escalation1);
            await component.Instance.ShowToast(escalation2);
            await component.Instance.ShowToast(escalation3); // Should remove first toast

            // Assert
            Assert.Equal(2, component.Instance.GetActiveToastCount()); // Should maintain max count
        }

        [Fact]
        public async Task EscalationToastContainer_DuplicateConversation_ShouldUpdateExisting()
        {
            // Arrange
            var component = RenderComponent<EscalationToastContainer>();
            var escalationData = CreateSampleEscalationData();

            // Act
            await component.Instance.ShowToast(escalationData);
            await component.Instance.ShowToast(escalationData); // Same conversation ID

            // Assert
            Assert.Equal(1, component.Instance.GetActiveToastCount()); // Should still be 1
        }

        [Fact]
        public async Task EscalationToastContainer_DismissToast_ShouldRemoveToast()
        {
            // Arrange
            var component = RenderComponent<EscalationToastContainer>();
            var escalationData = CreateSampleEscalationData();

            await component.Instance.ShowToast(escalationData);
            Assert.Equal(1, component.Instance.GetActiveToastCount());

            // Act
            await component.Instance.DismissToast(escalationData.ConversationReference);

            // Assert
            Assert.Equal(0, component.Instance.GetActiveToastCount());
            Assert.False(component.Instance.HasActiveToasts());
        }

        [Fact]
        public async Task EscalationToastContainer_DismissAllToasts_ShouldClearAll()
        {
            // Arrange
            var component = RenderComponent<EscalationToastContainer>();
            
            var escalation1 = CreateSampleEscalationData();
            escalation1.ConversationReference = "conv-1";
            
            var escalation2 = CreateSampleEscalationData();
            escalation2.ConversationReference = "conv-2";

            await component.Instance.ShowToast(escalation1);
            await component.Instance.ShowToast(escalation2);
            Assert.Equal(2, component.Instance.GetActiveToastCount());

            // Act
            await component.Instance.DismissAllToasts();

            // Assert
            Assert.Equal(0, component.Instance.GetActiveToastCount());
            Assert.False(component.Instance.HasActiveToasts());
        }

        [Fact]
        public async Task EscalationToastContainer_AcceptedCallback_ShouldTriggerSignalR()
        {
            // Arrange
            _mockSignalRService.Setup(x => x.IsConnected).Returns(true);
            _mockSignalRService.Setup(x => x.TryInvokeAsync("BroadcastEscalationAccepted", It.IsAny<string>()))
                .ReturnsAsync(true);

            var callbackTriggered = false;
            var component = RenderComponent<EscalationToastContainer>(parameters => parameters
                .Add(p => p.OnEscalationAccepted, EventCallback.Factory.Create<string>(this, _ => callbackTriggered = true)));

            var escalationData = CreateSampleEscalationData();
            await component.Instance.ShowToast(escalationData);

            // Act - Simulate toast acceptance (this would normally come from the toast component)
            var toastComponent = component.FindComponent<EscalationToast>();
            await toastComponent.Instance.OnAccepted.InvokeAsync(escalationData.ConversationReference);

            // Assert
            _mockSignalRService.Verify(x => x.TryInvokeAsync("BroadcastEscalationAccepted", 
                escalationData.ConversationReference), Times.Once);
            Assert.True(callbackTriggered);
        }

        [Fact]
        public void EscalationToastContainer_ResponsiveCSS_ShouldBeApplied()
        {
            // Act
            var component = RenderComponent<EscalationToastContainer>();

            // Assert
            var container = component.Find(".escalation-toast-container");
            Assert.NotNull(container);
            
            // Check that the container has the correct CSS class for positioning
            Assert.Contains("escalation-toast-container", container.GetClasses());
        }

        [Fact]
        public async Task EscalationToastContainer_NullEscalationData_ShouldNotAddToast()
        {
            // Arrange
            var component = RenderComponent<EscalationToastContainer>();

            // Act
            await component.Instance.ShowToast(null);

            // Assert
            Assert.Equal(0, component.Instance.GetActiveToastCount());
            Assert.False(component.Instance.HasActiveToasts());
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
