using CleanArchitecture.Blazor.Application.Common.Interfaces;

namespace IssueManager.Bot.Services
{
    public class MockApplicationHubWrapper : IApplicationHubWrapper
    {
        public async Task JobStarted(int id, string message)
        {
            // Mock implementation - could log the job start if needed
            await Task.CompletedTask;
        }

        public async Task JobCompleted(int id, string message)
        {
            // Mock implementation - could log the job completion if needed
            await Task.CompletedTask;
        }
    }
}