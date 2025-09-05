// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Infrastructure.Services.Jira;

/// <summary>
/// Configuration settings for JIRA integration
/// </summary>
public class JiraConfiguration
{
    public const string SectionName = "Jira";
    
    /// <summary>
    /// JIRA instance base URL (e.g., https://company.atlassian.net)
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// JIRA username/email for API authentication
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// JIRA API token for authentication
    /// </summary>
    public string ApiToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Default JIRA project key where issues will be created
    /// </summary>
    public string ProjectKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Default issue type ID for created issues
    /// </summary>
    public string IssueTypeId { get; set; } = "10001"; // Default Task
    
    /// <summary>
    /// Secret key for validating incoming webhooks
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;
    
    /// <summary>
    /// Timeout for JIRA API calls in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Maximum retry attempts for failed API calls
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
    
    /// <summary>
    /// Whether JIRA integration is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Field mapping configuration for custom JIRA fields
    /// </summary>
    public JiraFieldMapping FieldMapping { get; set; } = new();
}

/// <summary>
/// Configuration for mapping Issue fields to JIRA custom fields
/// </summary>
public class JiraFieldMapping
{
    /// <summary>
    /// JIRA field ID for mapping Issue Category
    /// </summary>
    public string? CategoryFieldId { get; set; }
    
    /// <summary>
    /// JIRA field ID for mapping Issue Priority
    /// </summary>
    public string? PriorityFieldId { get; set; }
    
    /// <summary>
    /// JIRA field ID for mapping Product/System
    /// </summary>
    public string? ProductFieldId { get; set; }
    
    /// <summary>
    /// JIRA field ID for mapping Severity
    /// </summary>
    public string? SeverityFieldId { get; set; }
    
    /// <summary>
    /// JIRA field ID for mapping Reporter Contact Information
    /// </summary>
    public string? ReporterContactFieldId { get; set; }
    
    /// <summary>
    /// JIRA field ID for mapping Original Channel (WhatsApp, etc.)
    /// </summary>
    public string? ChannelFieldId { get; set; }
}
