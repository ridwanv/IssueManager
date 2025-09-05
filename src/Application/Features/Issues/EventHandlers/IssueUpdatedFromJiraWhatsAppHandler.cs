using CleanArchitecture.Blazor.Domain.Events;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Blazor.Application.Features.Issues.EventHandlers;

public class IssueUpdatedFromJiraWhatsAppHandler : INotificationHandler<IssueUpdatedFromJiraEvent>
{
    private readonly ILogger<IssueUpdatedFromJiraWhatsAppHandler> _logger;

    public IssueUpdatedFromJiraWhatsAppHandler(ILogger<IssueUpdatedFromJiraWhatsAppHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(IssueUpdatedFromJiraEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing JIRA update notification for issue {IssueId} with JIRA key {JiraKey}", 
                notification.IssueId, notification.JiraKey);

            // Only send WhatsApp if we have a reporter phone number
            if (string.IsNullOrEmpty(notification.ReporterPhone))
            {
                _logger.LogWarning("No reporter phone number for issue {IssueId}, skipping WhatsApp notification", 
                    notification.IssueId);
                return;
            }

            // Create a human-readable message about the changes
            var message = CreateUpdateMessage(notification);

            // TODO: Integrate with WhatsApp Bot service to send the message
            // For now, we'll log the message that would be sent
            _logger.LogInformation("WhatsApp notification for {Phone}: {Message}", 
                notification.ReporterPhone, message);

            // In a real implementation, you would:
            // 1. Get the WhatsApp bot service
            // 2. Format the message appropriately
            // 3. Send the message to the reporter's phone number
            // 4. Handle any errors or delivery failures

            await SendWhatsAppMessage(notification.ReporterPhone, message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WhatsApp notification for JIRA update. Issue: {IssueId}", 
                notification.IssueId);
            // Don't throw - we don't want to fail the main update process if notifications fail
        }
    }

    private string CreateUpdateMessage(IssueUpdatedFromJiraEvent notification)
    {
        var messageParts = new List<string>
        {
            $"🔄 *Issue Update from JIRA*",
            $"Issue: {notification.JiraKey}",
            $"Current Status: {notification.CurrentStatus}"
        };

        if (notification.LocalUpdatedFields.Any())
        {
            messageParts.Add($"Updated: {string.Join(", ", notification.LocalUpdatedFields)}");
        }

        if (notification.JiraChangedFields.Any())
        {
            messageParts.Add($"JIRA Changes: {string.Join(", ", notification.JiraChangedFields)}");
        }

        messageParts.Add($"Updated: {DateTime.Now:HH:mm}");

        return string.Join("\n", messageParts);
    }

    private async Task SendWhatsAppMessage(string phoneNumber, string message, CancellationToken cancellationToken)
    {
        // TODO: Implement actual WhatsApp message sending
        // This would integrate with your WhatsApp Bot Framework
        
        _logger.LogInformation("Sending WhatsApp message to {Phone}: {Message}", phoneNumber, message);
        
        // Simulate async operation
        await Task.Delay(100, cancellationToken);
        
        // In a real implementation:
        // 1. Format phone number correctly
        // 2. Use WhatsApp Bot service to send message
        // 3. Handle delivery confirmation
        // 4. Retry logic for failed sends
        // 5. Rate limiting to avoid WhatsApp API limits
    }
}
