// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Events;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CleanArchitecture.Blazor.Application.Features.Issues.EventHandlers;

/// <summary>
/// Event handler that creates JIRA issues when local issues are created
/// </summary>
public class IssueCreatedJiraHandler : INotificationHandler<IssueCreatedEvent>
{
    private readonly IJiraService _jiraService;
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly ILogger<IssueCreatedJiraHandler> _logger;

    public IssueCreatedJiraHandler(
        IJiraService jiraService,
        IApplicationDbContextFactory dbContextFactory,
        ILogger<IssueCreatedJiraHandler> logger)
    {
        _jiraService = jiraService;
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task Handle(IssueCreatedEvent notification, CancellationToken cancellationToken)
    {
        var issue = notification.Item;
        
        try
        {
            _logger.LogInformation("Processing IssueCreatedEvent for issue {IssueId} - creating JIRA issue", 
                issue.Id);

            // Create issue in JIRA
            var jiraKey = await _jiraService.CreateIssueAsync(issue, cancellationToken);
            
            if (!string.IsNullOrEmpty(jiraKey))
            {
                // Update the local issue with JIRA information
                await UpdateIssueWithJiraInfo(issue.Id, jiraKey, cancellationToken);
                
                _logger.LogInformation("Successfully created JIRA issue {JiraKey} for local issue {IssueId}",
                    jiraKey, issue.Id);
            }
            else
            {
                _logger.LogWarning("JIRA issue creation returned empty key for local issue {IssueId}", 
                    issue.Id);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the transaction - JIRA integration is supplementary
            _logger.LogError(ex, "Failed to create JIRA issue for local issue {IssueId}. " +
                "This is non-critical and the local issue creation will continue.", issue.Id);
        }
    }

    private async Task UpdateIssueWithJiraInfo(Guid issueId, string jiraKey, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
            
            var issue = await db.Issues.FindAsync(new object[] { issueId }, cancellationToken);
            if (issue != null)
            {
                issue.JiraKey = jiraKey;
                issue.JiraUrl = BuildJiraUrl(jiraKey);
                issue.JiraCreatedAt = DateTime.UtcNow;
                issue.JiraLastSyncAt = DateTime.UtcNow;
                
                // Add event log for JIRA creation
                var eventLog = new Domain.Entities.EventLog
                {
                    Id = Guid.NewGuid(),
                    IssueId = issueId,
                    Type = "jira_created",
                    Payload = JsonSerializer.Serialize(new
                    {
                        JiraKey = jiraKey,
                        JiraUrl = issue.JiraUrl,
                        CreatedAt = DateTime.UtcNow
                    }),
                    CreatedUtc = DateTime.UtcNow,
                    TenantId = issue.TenantId
                };
                
                db.EventLogs.Add(eventLog);
                await db.SaveChangesAsync(cancellationToken);
                
                _logger.LogDebug("Updated local issue {IssueId} with JIRA key {JiraKey}", 
                    issueId, jiraKey);
            }
            else
            {
                _logger.LogWarning("Could not find local issue {IssueId} to update with JIRA info", 
                    issueId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update local issue {IssueId} with JIRA info for key {JiraKey}",
                issueId, jiraKey);
        }
    }

    private string BuildJiraUrl(string jiraKey)
    {
        // This will be injected from configuration later
        // For now, we'll construct it based on the JIRA service configuration
        return $"https://company.atlassian.net/browse/{jiraKey}";
    }
}
