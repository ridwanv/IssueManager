using CleanArchitecture.Blazor.Application.Features.Issues.Commands.Update;
using CleanArchitecture.Blazor.Application.Features.Issues.DTOs;
using CleanArchitecture.Blazor.Application.Common.Models;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace IssueManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JiraWebhookController : ControllerBase
{
    private readonly ILogger<JiraWebhookController> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _issuesApiBaseUrl;

    public JiraWebhookController(
        ILogger<JiraWebhookController> logger,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClient;
        _issuesApiBaseUrl = configuration.GetValue<string>("IssuesApi:BaseUrl") ?? "https://localhost:7001/api";
    }

    [HttpPost("issue-updated")]
    public async Task<IActionResult> HandleIssueUpdated([FromBody] JsonElement payload)
    {
        try
        {
            _logger.LogInformation("Received JIRA webhook: {Payload}", payload.ToString());

            // Validate webhook signature if configured
            if (!await ValidateWebhookSignature())
            {
                _logger.LogWarning("Invalid webhook signature received");
                return Unauthorized();
            }

            // Extract webhook event type
            if (!payload.TryGetProperty("webhookEvent", out var webhookEventElement))
            {
                _logger.LogWarning("Missing webhookEvent in payload");
                return BadRequest("Missing webhookEvent");
            }

            var webhookEvent = webhookEventElement.GetString();
            _logger.LogInformation("Processing webhook event: {Event}", webhookEvent);

            // Handle different webhook events
            switch (webhookEvent)
            {
                case "jira:issue_updated":
                    return await HandleJiraIssueUpdated(payload);
                case "jira:issue_deleted":
                    return await HandleJiraIssueDeleted(payload);
                default:
                    _logger.LogInformation("Ignoring webhook event: {Event}", webhookEvent);
                    return Ok("Event ignored");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing JIRA webhook");
            return StatusCode(500, "Internal server error");
        }
    }

    private async Task<IActionResult> HandleJiraIssueUpdated(JsonElement payload)
    {
        try
        {
            // Extract issue information
            if (!payload.TryGetProperty("issue", out var issueElement))
            {
                _logger.LogWarning("Missing issue in webhook payload");
                return BadRequest("Missing issue data");
            }

            if (!issueElement.TryGetProperty("key", out var keyElement))
            {
                _logger.LogWarning("Missing issue key in webhook payload");
                return BadRequest("Missing issue key");
            }

            var jiraKey = keyElement.GetString();
            if (string.IsNullOrEmpty(jiraKey))
            {
                _logger.LogWarning("Empty issue key in webhook payload");
                return BadRequest("Empty issue key");
            }

            _logger.LogInformation("Processing JIRA issue update for key: {JiraKey}", jiraKey);

            // Find the local issue by JiraKey
            var localIssue = await FindIssueByJiraKeyAsync(jiraKey);
            if (localIssue == null)
            {
                _logger.LogWarning("No local issue found for JIRA key: {JiraKey}", jiraKey);
                return NotFound($"No local issue found for JIRA key: {jiraKey}");
            }

            // Extract relevant fields from JIRA webhook
            var fieldsElement = issueElement.GetProperty("fields");
            var status = MapJiraStatusToLocalStatus(fieldsElement.TryGetProperty("status", out var statusElement) 
                ? statusElement.GetProperty("name").GetString() 
                : null);
            
            var priority = MapJiraPriorityToLocalPriority(fieldsElement.TryGetProperty("priority", out var priorityElement) 
                ? priorityElement.GetProperty("name").GetString() 
                : null);

            var summary = fieldsElement.TryGetProperty("summary", out var summaryElement) 
                ? summaryElement.GetString() 
                : null;

            var description = fieldsElement.TryGetProperty("description", out var descriptionElement) 
                ? descriptionElement.GetString() 
                : null;

            // Get changelog to determine what changed
            var changedFields = new List<string>();
            if (payload.TryGetProperty("changelog", out var changelogElement) &&
                changelogElement.TryGetProperty("items", out var itemsElement))
            {
                foreach (var item in itemsElement.EnumerateArray())
                {
                    if (item.TryGetProperty("field", out var fieldElement))
                    {
                        changedFields.Add(fieldElement.GetString() ?? "unknown");
                    }
                }
            }

            _logger.LogInformation("JIRA issue {JiraKey} changed fields: {ChangedFields}", 
                jiraKey, string.Join(", ", changedFields));

            // Create update command with only changed fields
            var updateCommand = new UpdateIssueCommand
            {
                Id = localIssue.Id,
                Title = summary ?? localIssue.Title,
                Description = description ?? localIssue.Description,
                Category = localIssue.Category,
                Priority = priority ?? localIssue.Priority,
                Status = status ?? localIssue.Status,
                ReporterContactId = localIssue.ReporterContactId,
                Channel = localIssue.Channel,
                Product = localIssue.Product,
                Severity = localIssue.Severity,
                Summary = localIssue.Summary,
                ConsentFlag = localIssue.ConsentFlag,
                DuplicateOfId = localIssue.DuplicateOfId
            };

            // Update local issue via API call
            var success = await UpdateIssueViaApiAsync(localIssue.Id, updateCommand);

            if (success)
            {
                _logger.LogInformation("Successfully updated local issue for JIRA key: {JiraKey}", jiraKey);
                return Ok("Issue updated successfully");
            }
            else
            {
                _logger.LogWarning("Failed to update local issue for JIRA key: {JiraKey}", jiraKey);
                return BadRequest($"Failed to update issue for JIRA key: {jiraKey}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling JIRA issue update webhook");
            return StatusCode(500, "Error processing issue update");
        }
    }

    private async Task<IActionResult> HandleJiraIssueDeleted(JsonElement payload)
    {
        try
        {
            // Extract issue key
            if (!payload.TryGetProperty("issue", out var issueElement) ||
                !issueElement.TryGetProperty("key", out var keyElement))
            {
                return BadRequest("Missing issue key");
            }

            var jiraKey = keyElement.GetString();
            _logger.LogInformation("Processing JIRA issue deletion for key: {JiraKey}", jiraKey);

            // Find the local issue by JiraKey
            var localIssue = await FindIssueByJiraKeyAsync(jiraKey);
            if (localIssue == null)
            {
                _logger.LogWarning("No local issue found for JIRA key: {JiraKey}", jiraKey);
                return NotFound($"No local issue found for JIRA key: {jiraKey}");
            }

            // Delete the local issue via API call
            var success = await DeleteIssueViaApiAsync(localIssue.Id);

            if (success)
            {
                _logger.LogInformation("Successfully deleted local issue for JIRA key: {JiraKey}", jiraKey);
                return Ok("Issue deletion processed");
            }
            else
            {
                _logger.LogWarning("Failed to delete local issue for JIRA key: {JiraKey}", jiraKey);
                return BadRequest($"Failed to delete issue for JIRA key: {jiraKey}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling JIRA issue deletion webhook");
            return StatusCode(500, "Error processing issue deletion");
        }
    }

    private async Task<bool> ValidateWebhookSignature()
    {
        // TODO: Implement webhook signature validation
        // For now, return true if no validation is configured
        return true;
    }

    /// <summary>
    /// Find an issue by JiraKey using the Issues API
    /// </summary>
    private async Task<IssueDto?> FindIssueByJiraKeyAsync(string jiraKey)
    {
        try
        {
            // Use the new endpoint to find by JIRA key
            var response = await _httpClient.GetAsync($"{_issuesApiBaseUrl}/issues/by-jira-key/{Uri.EscapeDataString(jiraKey)}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Result<IssueDto>>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                return result?.Data;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding issue by JIRA key: {JiraKey}", jiraKey);
            return null;
        }
    }

    /// <summary>
    /// Update an issue via the Issues API
    /// </summary>
    private async Task<bool> UpdateIssueViaApiAsync(Guid issueId, UpdateIssueCommand updateCommand)
    {
        try
        {
            var json = JsonSerializer.Serialize(updateCommand, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_issuesApiBaseUrl}/issues/{issueId}", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating issue via API: {IssueId}", issueId);
            return false;
        }
    }

    /// <summary>
    /// Delete an issue via the Issues API
    /// </summary>
    private async Task<bool> DeleteIssueViaApiAsync(Guid issueId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{_issuesApiBaseUrl}/issues/{issueId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting issue via API: {IssueId}", issueId);
            return false;
        }
    }

    /// <summary>
    /// Map JIRA status to local IssueStatus enum
    /// </summary>
    private IssueStatus? MapJiraStatusToLocalStatus(string? jiraStatus)
    {
        if (string.IsNullOrEmpty(jiraStatus))
            return null;

        return jiraStatus.ToLowerInvariant() switch
        {
            "to do" => IssueStatus.New,
            "open" => IssueStatus.New,
            "new" => IssueStatus.New,
            "in progress" => IssueStatus.InProgress,
            "done" => IssueStatus.Resolved,
            "closed" => IssueStatus.Closed,
            "resolved" => IssueStatus.Resolved,
            _ => IssueStatus.New // Default fallback
        };
    }

    /// <summary>
    /// Map JIRA priority to local IssuePriority enum
    /// </summary>
    private IssuePriority? MapJiraPriorityToLocalPriority(string? jiraPriority)
    {
        if (string.IsNullOrEmpty(jiraPriority))
            return null;

        return jiraPriority.ToLowerInvariant() switch
        {
            "highest" => IssuePriority.Critical,
            "high" => IssuePriority.High,
            "medium" => IssuePriority.Medium,
            "low" => IssuePriority.Low,
            "lowest" => IssuePriority.Low,
            "critical" => IssuePriority.Critical,
            _ => IssuePriority.Medium // Default fallback
        };
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
