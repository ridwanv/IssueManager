using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;
using IssueManager.Bot.Services;
using IssueManager.Bot.Models;

namespace Plugins;

public class IssueIntakePlugin
{
    private ConversationData _conversationData;
    private ITurnContext _turnContext;
    private IssueManagerApiClient _apiClient;

    public IssueIntakePlugin(ConversationData conversationData, ITurnContext<IMessageActivity> turnContext, IssueManagerApiClient apiClient)
    {
        _conversationData = conversationData;
        _turnContext = turnContext;
        _apiClient = apiClient;
    }

    [KernelFunction(name: "CollectIssueInformation")]
    [Description("CRITICAL: Use this function to collect, validate, and create issues from WhatsApp conversations. This is the PRIMARY function for processing user issue reports. Always ask for missing required fields before proceeding. Guide users through a conversational flow to collect complete issue information. Support both English and Afrikaans languages.")]
    public async Task<string> CollectIssueInformationAsync(
        [Description("REQUIRED: Contact information in format 'Name - +PhoneNumber' (e.g., 'John Doe - +27123456789'). If only name or phone provided, ask for the missing part. Must include country code.")] string? contact = null,
        [Description("REQUIRED: Specific product or system affected (e.g., 'Mobile Banking App', 'Online Portal', 'Policy Management System', 'Claims Processing'). Ask user to specify which system they're having trouble with.")] string? product = null,
        [Description("AUTO-DETERMINED: Issue severity level. Choose from: 'Critical' (system down, cannot access), 'High' (errors, broken features), 'Medium' (general technical issues), 'Low' (minor issues). System will auto-assign based on description if not provided.")] string? severity = null,
        [Description("AUTO-DETERMINED: Business priority level. Choose from: 'Urgent' (critical issues), 'High' (important fixes needed), 'Medium' (standard priority), 'Low' (nice-to-have). System will auto-assign based on severity and category if not provided.")] string? priority = null,
        [Description("AUTO-GENERATED: One-line summary of the issue (max 200 chars). System will generate from description if not provided. Should capture the essence of the problem clearly.")] string? summary = null,
        [Description("REQUIRED: Detailed description of the issue including: what went wrong, steps the user took, expected vs actual behavior, error messages seen, impact on user. Minimum 20 characters. Ask follow-up questions to get complete details.")] string? description = null,
        [Description("SUGGESTED: Issue category. Choose from: 'Technical' (system errors, bugs), 'Policy' (insurance policy issues), 'Claims' (claims processing), 'Billing' (payment issues), 'Account' (login, account access), 'General' (other issues). Will default to 'General' if not specified.")] string? category = null
    )
    {
        // Validate required fields and ask for missing information
        var missingFields = new List<string>();

        if (string.IsNullOrWhiteSpace(contact))
            missingFields.Add("your name and contact number");

        if (string.IsNullOrWhiteSpace(product))
            missingFields.Add("which product or system is affected");

        if (string.IsNullOrWhiteSpace(description))
            missingFields.Add("a detailed description of the issue");


        // If any required fields are missing, ask for them
        if (missingFields.Any())
        {
            return $"I need some additional information to log your issue:\n\n" +
                   $"Please provide: {string.Join(", ", missingFields)}.";
        }

        // Auto-assign severity and priority if not provided
        if (string.IsNullOrWhiteSpace(severity))
            severity = DetermineSeverity(description, category);

        if (string.IsNullOrWhiteSpace(priority))
            priority = DeterminePriority(severity, category);

        if (string.IsNullOrWhiteSpace(summary))
            summary = GenerateSummary(description);

        // Structure the issue data
        var issueData = new
        {
            Contact = contact,
            Product = product,
            Category = category ?? "General",
            Severity = severity,
            Priority = priority,
            Summary = summary,
            Description = description,
            Consent = true, // Auto-consent for WhatsApp issue intake
            ReportedAt = DateTime.UtcNow,
            Source = "WhatsApp",
            Attachments = _conversationData.Attachments?.Select(a => new { a.Name, a.ContentType, a.Url }).ToList()
        };

        try
        {
            // Parse category and priority enums
            var parsedCategory = ParseCategory(category ?? "General");
            var parsedPriority = ParsePriority(priority!);

            // Note: Similarity checking and auto-linking is now handled by the CreateIssueCommandHandler

            // Create the issue using API client instead of MediatR
            var command = new CreateIssueCommand
            {
                Title = summary!,
                Description = description!,
                Category = parsedCategory,
                Priority = parsedPriority,
                Status = IssueStatus.New,
                ReporterContactId = null, // Will be resolved by the API based on phone number
                ConversationId = _conversationData.ConversationEntityId, // Link to conversation
                Channel = "WhatsApp",
                Product = product!,
                Severity = severity!,
                Summary = summary!,
                ConsentFlag = true
            };

            var result = await _apiClient.CreateIssueAsync(command);
            
            if (result.Succeeded)
            {
                // Store success in conversation state for reference
                _conversationData.History.Add(new ConversationTurn
                {
                    Role = "system",
                    Message = $"ISSUE_CREATED:{result.Data}"
                });

                // Format confirmation message
                var confirmationMessage = FormatIssueConfirmation(issueData);
                var finalMessage = confirmationMessage + $"\n\n📋 **Issue ID:** {result.Data}";
                
                // Add note about automatic similarity detection
                finalMessage += "\n\n🔗 **Note:** Similar issues are automatically linked by our system for better tracking.";
                
                return finalMessage;
            }
            else
            {
                return $"There was a problem creating your issue: {result.ErrorMessage}. Please try again or contact support directly.";
            }
        }
        catch (Exception)
        {
            return "There was a problem processing your issue. Please try again or contact support directly.";
        }
    }

    [KernelFunction(name: "CheckIssueStatus")]
    [Description("Check the status of a previously created issue using the issue ID. Use this when users ask about the progress of their issues.")]
    public async Task<string> CheckIssueStatusAsync(
        [Description("The issue ID (GUID) to check status for")] string issueId
    )
    {
        try
        {
            if (!Guid.TryParse(issueId, out var guid))
            {
                return "Please provide a valid issue ID. Issue IDs are typically in the format: 12345678-1234-1234-1234-123456789012";
            }

            var result = await _apiClient.GetIssueAsync(guid);
            
            if (result.Succeeded && result.Data != null)
            {
                var issue = result.Data;
                var statusMessage = $"📋 **Issue Status Update**\n\n";
                statusMessage += $"**Issue ID:** {issue.Id}\n";
                statusMessage += $"**Reference:** {issue.ReferenceNumber}\n";
                statusMessage += $"**Title:** {issue.Title}\n";
                statusMessage += $"**Status:** {GetStatusEmoji(issue.Status)} {issue.Status}\n";
                statusMessage += $"**Priority:** {GetPriorityEmoji(issue.Priority)} {issue.Priority}\n";
                statusMessage += $"**Category:** {issue.Category}\n";
                
                if (issue.Created.HasValue)
                {
                    statusMessage += $"**Created:** {issue.Created.Value:yyyy-MM-dd HH:mm}\n";
                }
                
                if (issue.LastModified.HasValue)
                {
                    statusMessage += $"**Last Updated:** {issue.LastModified.Value:yyyy-MM-dd HH:mm}\n";
                }

                statusMessage += $"\n**Description:** {issue.Description}";

                return statusMessage;
            }
            else
            {
                return $"I couldn't find an issue with ID: {issueId}. Please check the ID and try again.";
            }
        }
        catch (Exception ex)
        {
            return "There was a problem checking the issue status. Please try again later.";
        }
    }

    [KernelFunction(name: "CheckForDuplicateIssue")]
    [Description("OPTIONAL: Check if a similar issue has been reported recently by the same contact within the last 7 days to prevent duplicate issue creation. Use this before creating a new issue if you suspect the user might have already reported this problem.")]
    public async Task<string> CheckForDuplicateIssueAsync(
        [Description("Phone number of the reporting user in E.164 format (e.g., +27123456789)")] string phoneNumber,
        [Description("Detailed description of the current issue being reported for similarity comparison")] string currentDescription
    )
    {
        try
        {
            // Get recent issues to check for duplicates
            var recentIssues = await _apiClient.GetIssuesAsync(
                pageNumber: 1, 
                pageSize: 50, 
                keyword: phoneNumber
            );

            if (recentIssues?.Items?.Any() == true)
            {
                // Check for issues from the last 7 days
                var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
                var recentByPhone = recentIssues.Items
                    .Where(i => i.Created.HasValue && i.Created.Value >= sevenDaysAgo)
                    .Where(i => !string.IsNullOrEmpty(i.ReporterPhone) && i.ReporterPhone.Contains(phoneNumber.Replace("+", "")))
                    .ToList();

                if (recentByPhone.Any())
                {
                    var duplicateMessage = $"⚠️ **Potential Duplicate Issues Found**\n\n";
                    duplicateMessage += $"I found {recentByPhone.Count} recent issue(s) from this number:\n\n";
                    
                    foreach (var issue in recentByPhone.Take(3)) // Show max 3 recent issues
                    {
                        duplicateMessage += $"• **{issue.ReferenceNumber}** - {issue.Title} ({issue.Status})\n";
                    }
                    
                    duplicateMessage += "\nWould you like to update an existing issue instead of creating a new one?";
                    return duplicateMessage;
                }
            }

            return "No recent duplicate issues found. Proceeding with new issue creation.";
        }
        catch (Exception ex)
        {
            return "Unable to check for duplicate issues at this time. Proceeding with new issue creation.";
        }
    }

    private IssueCategory ParseCategory(string category)
    {
        return category?.ToLowerInvariant() switch
        {
            "technical" => IssueCategory.Technical,
            "billing" => IssueCategory.Billing,
            "feature" => IssueCategory.Feature,
            "general" => IssueCategory.General,
            _ => IssueCategory.General
        };
    }

    private IssuePriority ParsePriority(string priority)
    {
        return priority?.ToLowerInvariant() switch
        {
            "urgent" or "critical" => IssuePriority.Critical,
            "high" => IssuePriority.High,
            "medium" => IssuePriority.Medium,
            "low" => IssuePriority.Low,
            _ => IssuePriority.Medium
        };
    }

    private string GetStatusEmoji(IssueStatus status)
    {
        return status switch
        {
            IssueStatus.New => "🆕",
            IssueStatus.InProgress => "⏳",
            IssueStatus.Resolved => "✅",
            IssueStatus.Closed => "🔒",
            IssueStatus.OnHold => "⏸️",
            _ => "❓"
        };
    }

    private string GetPriorityEmoji(IssuePriority priority)
    {
        return priority switch
        {
            IssuePriority.Critical => "🔴",
            IssuePriority.High => "🟠",
            IssuePriority.Medium => "🟡",
            IssuePriority.Low => "🟢",
            _ => "⚪"
        };
    }

    private string DetermineSeverity(string description, string? category)
    {
        var descriptionLower = description?.ToLowerInvariant() ?? "";

        // Critical indicators
        if (descriptionLower.Contains("cannot access") ||
            descriptionLower.Contains("system down") ||
            descriptionLower.Contains("urgent") ||
            descriptionLower.Contains("emergency"))
            return "Critical";

        // High severity indicators
        if (descriptionLower.Contains("error") ||
            descriptionLower.Contains("broken") ||
            descriptionLower.Contains("not working") ||
            category == "Claims")
            return "High";

        // Medium for most issues
        if (category == "Technical" || category == "Policy")
            return "Medium";

        return "Low";
    }

    private string DeterminePriority(string severity, string? category)
    {
        return severity switch
        {
            "Critical" => "Urgent",
            "High" => category == "Claims" ? "High" : "Medium",
            "Medium" => "Medium",
            _ => "Low"
        };
    }

    private string GenerateSummary(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return "Issue reported via WhatsApp";

        // Simple summary generation - take first sentence or truncate
        var sentences = description.Split('.', '!', '?');
        var firstSentence = sentences.FirstOrDefault()?.Trim();

        if (string.IsNullOrWhiteSpace(firstSentence))
            return "Issue reported via WhatsApp";

        return firstSentence.Length > 100
            ? $"{firstSentence.Substring(0, 97)}..."
            : firstSentence;
    }

    private string FormatIssueConfirmation(object issueData)
    {
        var data = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(issueData));

        var message = "✅ **Issue Successfully Created**\n\n";
        message += $"📞 **Contact:** {data.GetProperty("Contact").GetString()}\n";
        message += $"🏢 **Product/System:** {data.GetProperty("Product").GetString()}\n";
        message += $"📂 **Category:** {data.GetProperty("Category").GetString()}\n";
        message += $"⚠️ **Severity:** {data.GetProperty("Severity").GetString()}\n";
        message += $"🔥 **Priority:** {data.GetProperty("Priority").GetString()}\n";
        message += $"📝 **Summary:** {data.GetProperty("Summary").GetString()}\n\n";
        message += $"**Description:** {data.GetProperty("Description").GetString()}\n\n";

        if (data.GetProperty("Attachments").GetArrayLength() > 0)
        {
            message += $"📎 **Attachments:** {data.GetProperty("Attachments").GetArrayLength()} file(s)\n\n";
        }

        message += "Your issue will be processed and you'll receive updates on its progress.\n";
        message += "Reference this conversation for any follow-up questions.";

        return message;
    }

}