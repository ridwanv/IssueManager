// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Infrastructure.Services.Jira.Models;
using CleanArchitecture.Blazor.Application.Common.Interfaces;

namespace CleanArchitecture.Blazor.Infrastructure.Services.Jira;

/// <summary>
/// Extended service interface for JIRA integration operations - Infrastructure layer implementation
/// </summary>
public interface IJiraServiceExtended : CleanArchitecture.Blazor.Application.Common.Interfaces.IJiraService
{
    /// <summary>
    /// Gets a JIRA issue by its key
    /// </summary>
    /// <param name="jiraKey">JIRA issue key (e.g., "SUP-123")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JIRA issue details</returns>
    Task<JiraIssue?> GetIssueAsync(string jiraKey, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates a JIRA issue
    /// </summary>
    /// <param name="jiraKey">JIRA issue key</param>
    /// <param name="updateRequest">Update request with fields to change</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> UpdateIssueAsync(string jiraKey, UpdateJiraIssueRequest updateRequest, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates an incoming webhook payload and signature
    /// </summary>
    /// <param name="payload">Raw webhook payload</param>
    /// <param name="signature">Webhook signature header</param>
    /// <returns>True if valid</returns>
    Task<bool> ValidateWebhookAsync(string payload, string? signature);
    
    /// <summary>
    /// Processes a JIRA webhook payload and extracts issue update information
    /// </summary>
    /// <param name="payload">Webhook payload JSON</param>
    /// <returns>Parsed webhook data</returns>
    Task<JiraWebhookData?> ProcessWebhookAsync(string payload);
    
    /// <summary>
    /// Gets available JIRA projects for the configured user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available projects</returns>
    Task<List<JiraProject>> GetProjectsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets available issue types for a specific project
    /// </summary>
    /// <param name="projectKey">JIRA project key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available issue types</returns>
    Task<List<JiraIssueType>> GetIssueTypesAsync(string projectKey, CancellationToken cancellationToken = default);
}
