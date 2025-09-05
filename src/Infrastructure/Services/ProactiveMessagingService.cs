using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CleanArchitecture.Blazor.Infrastructure.Services;

/// <summary>
/// Service for sending proactive WhatsApp messages about issue updates
/// </summary>
public class ProactiveMessagingService : IProactiveMessagingService
{
    private readonly ILogger<ProactiveMessagingService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ProactiveMessagingService(
        ILogger<ProactiveMessagingService> logger,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task SendIssueCreatedMessageAsync(Issue issue, CancellationToken cancellationToken = default)
    {
        if (issue.ReporterContact?.PhoneNumber == null)
        {
            _logger.LogWarning("Cannot send proactive message for issue {IssueId} - no reporter phone number", issue.Id);
            return;
        }

        var message = $"✅ Your issue has been successfully created!\n\n" +
                     $"📋 Reference: {issue.ReferenceNumber}\n" +
                     $"📝 Title: {issue.Title}\n" +
                     $"📊 Priority: {issue.Priority}\n" +
                     $"🏷️ Category: {issue.Category}\n\n" +
                     $"We'll keep you updated on the progress. Thank you for reporting this issue!";

        await SendWhatsAppMessageAsync(issue.ReporterContact.PhoneNumber, message, cancellationToken);
    }

    public async Task SendIssueStatusChangedMessageAsync(Issue issue, IssueStatus previousStatus, IssueStatus newStatus, CancellationToken cancellationToken = default)
    {
        if (issue.ReporterContact?.PhoneNumber == null)
        {
            _logger.LogWarning("Cannot send status change message for issue {IssueId} - no reporter phone number", issue.Id);
            return;
        }

        var statusEmoji = newStatus switch
        {
            IssueStatus.InProgress => "🔄",
            IssueStatus.Resolved => "✅",
            IssueStatus.Closed => "🔒",
            IssueStatus.OnHold => "⏸️",
            _ => "📋"
        };

        var message = $"{statusEmoji} Status Update for your issue\n\n" +
                     $"📋 Reference: {issue.ReferenceNumber}\n" +
                     $"📝 Title: {issue.Title}\n" +
                     $"📊 Status changed from: {previousStatus} → {newStatus}\n\n" +
                     $"We'll continue to keep you updated on any changes.";

        await SendWhatsAppMessageAsync(issue.ReporterContact.PhoneNumber, message, cancellationToken);
    }

    public async Task SendIssueAssignedMessageAsync(Issue issue, string? previousAssignedUserId, string? newAssignedUserId, CancellationToken cancellationToken = default)
    {
        if (issue.ReporterContact?.PhoneNumber == null)
        {
            _logger.LogWarning("Cannot send assignment message for issue {IssueId} - no reporter phone number", issue.Id);
            return;
        }

        var message = newAssignedUserId != null
            ? $"👤 Your issue has been assigned to a team member\n\n" +
              $"📋 Reference: {issue.ReferenceNumber}\n" +
              $"📝 Title: {issue.Title}\n\n" +
              $"A dedicated team member will now be working on your issue. You'll receive updates as progress is made."
            : $"📋 Your issue assignment has been updated\n\n" +
              $"📋 Reference: {issue.ReferenceNumber}\n" +
              $"📝 Title: {issue.Title}\n\n" +
              $"The issue has been reassigned to ensure the best possible resolution.";

        await SendWhatsAppMessageAsync(issue.ReporterContact.PhoneNumber, message, cancellationToken);
    }

    public async Task SendIssueResolvedMessageAsync(Issue issue, string? resolutionNotes = null, CancellationToken cancellationToken = default)
    {
        if (issue.ReporterContact?.PhoneNumber == null)
        {
            _logger.LogWarning("Cannot send resolution message for issue {IssueId} - no reporter phone number", issue.Id);
            return;
        }

        var message = $"🎉 Great news! Your issue has been resolved\n\n" +
                     $"📋 Reference: {issue.ReferenceNumber}\n" +
                     $"📝 Title: {issue.Title}\n";

        if (!string.IsNullOrEmpty(resolutionNotes))
        {
            message += $"\n💬 Resolution notes:\n{resolutionNotes}\n";
        }

        message += $"\nIf you have any questions or if the issue persists, please don't hesitate to contact us again. Thank you for your patience!";

        await SendWhatsAppMessageAsync(issue.ReporterContact.PhoneNumber, message, cancellationToken);
    }

    public async Task SendIssueCommentAddedMessageAsync(Issue issue, string comment, string addedByUserId, CancellationToken cancellationToken = default)
    {
        if (issue.ReporterContact?.PhoneNumber == null)
        {
            _logger.LogWarning("Cannot send comment message for issue {IssueId} - no reporter phone number", issue.Id);
            return;
        }

        var message = $"💬 New update on your issue\n\n" +
                     $"📋 Reference: {issue.ReferenceNumber}\n" +
                     $"📝 Title: {issue.Title}\n\n" +
                     $"📝 Update:\n{comment}\n\n" +
                     $"Thank you for your patience as we work to resolve your issue.";

        await SendWhatsAppMessageAsync(issue.ReporterContact.PhoneNumber, message, cancellationToken);
    }

    private async Task SendWhatsAppMessageAsync(string phoneNumber, string message, CancellationToken cancellationToken)
    {
        try
        {
            var botBaseUrl = _configuration.GetValue<string>("Bot:BaseUrl") ?? "http://localhost:5000";
            var endpoint = $"{botBaseUrl}/api/proactive-message";

            var payload = new
            {
                phoneNumber = phoneNumber,
                message = message
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Proactive WhatsApp message sent successfully to {PhoneNumber}", phoneNumber);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to send proactive WhatsApp message to {PhoneNumber}. Status: {StatusCode}, Response: {Response}",
                    phoneNumber, response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending proactive WhatsApp message to {PhoneNumber}", phoneNumber);
        }
    }
}