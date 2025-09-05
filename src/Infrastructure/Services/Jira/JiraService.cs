// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using CleanArchitecture.Blazor.Infrastructure.Services.Jira.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Blazor.Infrastructure.Services.Jira;

/// <summary>
/// Implementation of JIRA integration service
/// </summary>
public class JiraService : IJiraServiceExtended
{
    private readonly HttpClient _httpClient;
    private readonly JiraConfiguration _config;
    private readonly ILogger<JiraService> _logger;
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public JiraService(
        HttpClient httpClient,
        IOptions<JiraConfiguration> config,
        ILogger<JiraService> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
        
        // Configure HTTP client
        ConfigureHttpClient();
    }

    public async Task<string> CreateIssueAsync(Issue issue, CancellationToken cancellationToken = default)
    {
        if (!_config.Enabled)
        {
            _logger.LogInformation("JIRA integration is disabled, skipping issue creation");
            return string.Empty;
        }

        try
        {
            _logger.LogInformation("Creating JIRA issue for local issue {IssueId}", issue.Id);

            var request = BuildCreateIssueRequest(issue);
            var json = JsonSerializer.Serialize(request, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/rest/api/3/issue", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                var jiraResponse = JsonSerializer.Deserialize<JiraApiResponse<object>>(responseJson, JsonOptions);
                
                var jiraKey = jiraResponse?.Key ?? string.Empty;
                _logger.LogInformation("Successfully created JIRA issue {JiraKey} for local issue {IssueId}",
                    jiraKey, issue.Id);
                
                return jiraKey;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to create JIRA issue. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);
                
                throw new InvalidOperationException($"JIRA API error: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating JIRA issue for local issue {IssueId}", issue.Id);
            throw;
        }
    }

    public async Task<JiraIssue?> GetIssueAsync(string jiraKey, CancellationToken cancellationToken = default)
    {
        if (!_config.Enabled)
            return null;

        try
        {
            _logger.LogDebug("Fetching JIRA issue {JiraKey}", jiraKey);

            var response = await _httpClient.GetAsync($"/rest/api/3/issue/{jiraKey}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var jiraIssue = JsonSerializer.Deserialize<JiraIssue>(json, JsonOptions);
                
                _logger.LogDebug("Successfully fetched JIRA issue {JiraKey}", jiraKey);
                return jiraIssue;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("JIRA issue {JiraKey} not found", jiraKey);
                return null;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to fetch JIRA issue {JiraKey}. Status: {StatusCode}, Error: {Error}",
                    jiraKey, response.StatusCode, errorContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching JIRA issue {JiraKey}", jiraKey);
            return null;
        }
    }

    public async Task<bool> UpdateIssueAsync(string jiraKey, UpdateJiraIssueRequest updateRequest, CancellationToken cancellationToken = default)
    {
        if (!_config.Enabled)
            return false;

        try
        {
            _logger.LogInformation("Updating JIRA issue {JiraKey}", jiraKey);

            var json = JsonSerializer.Serialize(updateRequest, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/rest/api/3/issue/{jiraKey}", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated JIRA issue {JiraKey}", jiraKey);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to update JIRA issue {JiraKey}. Status: {StatusCode}, Error: {Error}",
                    jiraKey, response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating JIRA issue {JiraKey}", jiraKey);
            return false;
        }
    }

    public Task<bool> ValidateWebhookAsync(string payload, string? signature)
    {
        if (string.IsNullOrEmpty(_config.WebhookSecret))
        {
            _logger.LogWarning("Webhook secret not configured, skipping validation");
            return Task.FromResult(true); // Allow if no secret configured
        }

        if (string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Webhook signature missing");
            return Task.FromResult(false);
        }

        try
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config.WebhookSecret));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = Convert.ToHexString(computedHash).ToLowerInvariant();
            
            // JIRA typically sends signature as "sha256=<hash>"
            var expectedSignature = signature.StartsWith("sha256=") 
                ? signature[7..] 
                : signature;
            
            var isValid = string.Equals(computedSignature, expectedSignature, StringComparison.OrdinalIgnoreCase);
            
            if (!isValid)
            {
                _logger.LogWarning("Webhook signature validation failed");
            }
            
            return Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating webhook signature");
            return Task.FromResult(false);
        }
    }

    public Task<JiraWebhookData?> ProcessWebhookAsync(string payload)
    {
        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            
            var webhookData = new JiraWebhookData
            {
                EventType = root.GetProperty("webhookEvent").GetString() ?? string.Empty,
                Timestamp = DateTime.UtcNow
            };
            
            if (root.TryGetProperty("issue", out var issueElement))
            {
                webhookData.IssueKey = issueElement.GetProperty("key").GetString() ?? string.Empty;
                webhookData.IssueId = issueElement.GetProperty("id").GetString() ?? string.Empty;
                
                if (issueElement.TryGetProperty("fields", out var fieldsElement))
                {
                    // Extract status
                    if (fieldsElement.TryGetProperty("status", out var statusElement))
                    {
                        webhookData.Status = JsonSerializer.Deserialize<JiraStatus>(statusElement.GetRawText(), JsonOptions);
                    }
                    
                    // Extract assignee
                    if (fieldsElement.TryGetProperty("assignee", out var assigneeElement) && 
                        assigneeElement.ValueKind != JsonValueKind.Null)
                    {
                        webhookData.Assignee = JsonSerializer.Deserialize<JiraUser>(assigneeElement.GetRawText(), JsonOptions);
                    }
                }
            }
            
            // Extract change author
            if (root.TryGetProperty("user", out var userElement))
            {
                webhookData.ChangeAuthor = JsonSerializer.Deserialize<JiraUser>(userElement.GetRawText(), JsonOptions);
            }
            
            // Extract changes
            if (root.TryGetProperty("changelog", out var changelogElement) &&
                changelogElement.TryGetProperty("items", out var itemsElement))
            {
                foreach (var item in itemsElement.EnumerateArray())
                {
                    webhookData.Changes.Add(new JiraFieldChange
                    {
                        Field = item.GetProperty("field").GetString() ?? string.Empty,
                        FromValue = item.TryGetProperty("fromString", out var fromProp) ? fromProp.GetString() : null,
                        ToValue = item.TryGetProperty("toString", out var toProp) ? toProp.GetString() : null
                    });
                }
            }
            
            return Task.FromResult<JiraWebhookData?>(webhookData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing JIRA webhook payload");
            return Task.FromResult<JiraWebhookData?>(null);
        }
    }

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (!_config.Enabled)
            return false;

        try
        {
            _logger.LogInformation("Testing JIRA connection");

            var response = await _httpClient.GetAsync("/rest/api/3/myself", cancellationToken);
            var isConnected = response.IsSuccessStatusCode;
            
            if (isConnected)
            {
                _logger.LogInformation("JIRA connection test successful");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("JIRA connection test failed. Status: {StatusCode}, Error: {Error}",
                    response.StatusCode, errorContent);
            }
            
            return isConnected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing JIRA connection");
            return false;
        }
    }

    public async Task<List<JiraProject>> GetProjectsAsync(CancellationToken cancellationToken = default)
    {
        if (!_config.Enabled)
            return new List<JiraProject>();

        try
        {
            var response = await _httpClient.GetAsync("/rest/api/3/project", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var projects = JsonSerializer.Deserialize<List<JiraProject>>(json, JsonOptions);
                return projects ?? new List<JiraProject>();
            }
            
            return new List<JiraProject>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching JIRA projects");
            return new List<JiraProject>();
        }
    }

    public async Task<List<JiraIssueType>> GetIssueTypesAsync(string projectKey, CancellationToken cancellationToken = default)
    {
        if (!_config.Enabled)
            return new List<JiraIssueType>();

        try
        {
            var response = await _httpClient.GetAsync($"/rest/api/3/project/{projectKey}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                using var document = JsonDocument.Parse(json);
                
                if (document.RootElement.TryGetProperty("issueTypes", out var issueTypesElement))
                {
                    var issueTypes = JsonSerializer.Deserialize<List<JiraIssueType>>(issueTypesElement.GetRawText(), JsonOptions);
                    return issueTypes ?? new List<JiraIssueType>();
                }
            }
            
            return new List<JiraIssueType>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching issue types for project {ProjectKey}", projectKey);
            return new List<JiraIssueType>();
        }
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
        
        // Set up basic authentication
        var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_config.Username}:{_config.ApiToken}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        
        // Set headers
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "IssueManager-Integration/1.0");
    }

    private CreateJiraIssueRequest BuildCreateIssueRequest(Issue issue)
    {
        var fields = new Dictionary<string, object>
        {
            ["project"] = new { key = _config.ProjectKey },
            ["issuetype"] = new { id = _config.IssueTypeId },
            ["summary"] = issue.Title,
            ["description"] = JiraContent.CreateText(BuildIssueDescription(issue))
        };

        // Map priority
        if (!string.IsNullOrEmpty(issue.Severity))
        {
            var jiraPriority = MapPriorityToJira(issue.Priority);
            if (!string.IsNullOrEmpty(jiraPriority))
            {
                fields["priority"] = new { name = jiraPriority };
            }
        }

        // Add custom fields based on configuration
        AddCustomFields(fields, issue);

        return new CreateJiraIssueRequest { Fields = fields };
    }

    private string BuildIssueDescription(Issue issue)
    {
        var description = new StringBuilder();
        description.AppendLine($"Issue Details:");
        description.AppendLine($"Reference: {issue.ReferenceNumber}");
        description.AppendLine($"Category: {issue.Category}");
        description.AppendLine($"Priority: {issue.Priority}");
        
        if (!string.IsNullOrEmpty(issue.Product))
            description.AppendLine($"Product/System: {issue.Product}");
        
        if (!string.IsNullOrEmpty(issue.Severity))
            description.AppendLine($"Severity: {issue.Severity}");
        
        if (!string.IsNullOrEmpty(issue.Channel))
            description.AppendLine($"Channel: {issue.Channel}");
        
        if (issue.ReporterContact != null)
        {
            description.AppendLine($"Reporter: {issue.ReporterContact.Name}");
            if (!string.IsNullOrEmpty(issue.ReporterContact.PhoneNumber))
                description.AppendLine($"Phone: {issue.ReporterContact.PhoneNumber}");
        }
        
        description.AppendLine();
        description.AppendLine("Description:");
        description.AppendLine(issue.Description);
        
        return description.ToString();
    }

    private string? MapPriorityToJira(IssuePriority priority)
    {
        return priority switch
        {
            IssuePriority.Critical => "Highest",
            IssuePriority.High => "High",
            IssuePriority.Medium => "Medium",
            IssuePriority.Low => "Low",
            _ => "Medium"
        };
    }

    private void AddCustomFields(Dictionary<string, object> fields, Issue issue)
    {
        var mapping = _config.FieldMapping;
        
        if (!string.IsNullOrEmpty(mapping.CategoryFieldId))
            fields[mapping.CategoryFieldId] = issue.Category.ToString();
        
        if (!string.IsNullOrEmpty(mapping.ProductFieldId) && !string.IsNullOrEmpty(issue.Product))
            fields[mapping.ProductFieldId] = issue.Product;
        
        if (!string.IsNullOrEmpty(mapping.SeverityFieldId) && !string.IsNullOrEmpty(issue.Severity))
            fields[mapping.SeverityFieldId] = issue.Severity;
        
        if (!string.IsNullOrEmpty(mapping.ChannelFieldId) && !string.IsNullOrEmpty(issue.Channel))
            fields[mapping.ChannelFieldId] = issue.Channel;
        
        if (!string.IsNullOrEmpty(mapping.ReporterContactFieldId) && issue.ReporterContact != null)
        {
            var contactInfo = $"{issue.ReporterContact.Name}";
            if (!string.IsNullOrEmpty(issue.ReporterContact.PhoneNumber))
                contactInfo += $" - {issue.ReporterContact.PhoneNumber}";
            fields[mapping.ReporterContactFieldId] = contactInfo;
        }
    }
}
