// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Serialization;

namespace CleanArchitecture.Blazor.Infrastructure.Services.Jira.Models;

/// <summary>
/// JIRA issue representation for API calls
/// </summary>
public class JiraIssue
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;
    
    [JsonPropertyName("self")]
    public string Self { get; set; } = string.Empty;
    
    [JsonPropertyName("fields")]
    public JiraIssueFields Fields { get; set; } = new();
}

/// <summary>
/// JIRA issue fields
/// </summary>
public class JiraIssueFields
{
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public JiraContent? Description { get; set; }
    
    [JsonPropertyName("issuetype")]
    public JiraIssueType IssueType { get; set; } = new();
    
    [JsonPropertyName("project")]
    public JiraProject Project { get; set; } = new();
    
    [JsonPropertyName("priority")]
    public JiraPriority? Priority { get; set; }
    
    [JsonPropertyName("status")]
    public JiraStatus? Status { get; set; }
    
    [JsonPropertyName("assignee")]
    public JiraUser? Assignee { get; set; }
    
    [JsonPropertyName("reporter")]
    public JiraUser? Reporter { get; set; }
    
    [JsonPropertyName("created")]
    public DateTime? Created { get; set; }
    
    [JsonPropertyName("updated")]
    public DateTime? Updated { get; set; }
    
    [JsonPropertyName("resolutiondate")]
    public DateTime? ResolutionDate { get; set; }
    
    /// <summary>
    /// Custom fields - will be populated dynamically based on configuration
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object> CustomFields { get; set; } = new();
}

/// <summary>
/// JIRA issue type
/// </summary>
public class JiraIssueType
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("iconUrl")]
    public string? IconUrl { get; set; }
}

/// <summary>
/// JIRA project information
/// </summary>
public class JiraProject
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// JIRA priority
/// </summary>
public class JiraPriority
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("iconUrl")]
    public string? IconUrl { get; set; }
}

/// <summary>
/// JIRA status
/// </summary>
public class JiraStatus
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("statusCategory")]
    public JiraStatusCategory? StatusCategory { get; set; }
}

/// <summary>
/// JIRA status category
/// </summary>
public class JiraStatusCategory
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;
}

/// <summary>
/// JIRA user
/// </summary>
public class JiraUser
{
    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;
    
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;
    
    [JsonPropertyName("emailAddress")]
    public string? EmailAddress { get; set; }
}

/// <summary>
/// JIRA content (for description and comments)
/// </summary>
public class JiraContent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "doc";
    
    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;
    
    [JsonPropertyName("content")]
    public List<JiraContentNode> Content { get; set; } = new();
    
    /// <summary>
    /// Creates a simple text content for JIRA description
    /// </summary>
    public static JiraContent CreateText(string text)
    {
        return new JiraContent
        {
            Content = new List<JiraContentNode>
            {
                new()
                {
                    Type = "paragraph",
                    Content = new List<JiraContentNode>
                    {
                        new()
                        {
                            Type = "text",
                            Text = text
                        }
                    }
                }
            }
        };
    }
}

/// <summary>
/// JIRA content node (for rich text content)
/// </summary>
public class JiraContentNode
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("text")]
    public string? Text { get; set; }
    
    [JsonPropertyName("content")]
    public List<JiraContentNode>? Content { get; set; }
    
    [JsonPropertyName("attrs")]
    public Dictionary<string, object>? Attrs { get; set; }
}

/// <summary>
/// Request model for creating JIRA issues
/// </summary>
public class CreateJiraIssueRequest
{
    [JsonPropertyName("fields")]
    public Dictionary<string, object> Fields { get; set; } = new();
}

/// <summary>
/// Request model for updating JIRA issues
/// </summary>
public class UpdateJiraIssueRequest
{
    [JsonPropertyName("fields")]
    public Dictionary<string, object> Fields { get; set; } = new();
}

/// <summary>
/// Response model for JIRA API operations
/// </summary>
public class JiraApiResponse<T>
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("key")]
    public string? Key { get; set; }
    
    [JsonPropertyName("self")]
    public string? Self { get; set; }
    
    [JsonPropertyName("data")]
    public T? Data { get; set; }
    
    [JsonPropertyName("errorMessages")]
    public List<string>? ErrorMessages { get; set; }
    
    [JsonPropertyName("errors")]
    public Dictionary<string, string>? Errors { get; set; }
}

/// <summary>
/// Data extracted from JIRA webhook payloads
/// </summary>
public class JiraWebhookData
{
    /// <summary>
    /// Type of webhook event (issue_created, issue_updated, etc.)
    /// </summary>
    public string EventType { get; set; } = string.Empty;
    
    /// <summary>
    /// JIRA issue key
    /// </summary>
    public string IssueKey { get; set; } = string.Empty;
    
    /// <summary>
    /// JIRA issue ID
    /// </summary>
    public string IssueId { get; set; } = string.Empty;
    
    /// <summary>
    /// Current status of the issue
    /// </summary>
    public JiraStatus? Status { get; set; }
    
    /// <summary>
    /// Issue assignee information
    /// </summary>
    public JiraUser? Assignee { get; set; }
    
    /// <summary>
    /// Resolution information if issue is resolved
    /// </summary>
    public string? Resolution { get; set; }
    
    /// <summary>
    /// Timestamp of the change
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// User who made the change
    /// </summary>
    public JiraUser? ChangeAuthor { get; set; }
    
    /// <summary>
    /// Summary of changes made
    /// </summary>
    public List<JiraFieldChange> Changes { get; set; } = new();
    
    /// <summary>
    /// Comment added (if any)
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Represents a field change in JIRA webhook
/// </summary>
public class JiraFieldChange
{
    /// <summary>
    /// Field name that changed
    /// </summary>
    public string Field { get; set; } = string.Empty;
    
    /// <summary>
    /// Previous value
    /// </summary>
    public string? FromValue { get; set; }
    
    /// <summary>
    /// New value
    /// </summary>
    public string? ToValue { get; set; }
}
