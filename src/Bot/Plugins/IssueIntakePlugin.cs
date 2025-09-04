using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;
using MediatR;
using CleanArchitecture.Blazor.Application.Features.Issues.Commands.Create;

namespace Plugins;

public class IssueIntakePlugin
{
    private ConversationData _conversationData;
    private ITurnContext _turnContext;
    private IConfiguration _config;
    private IMediator _mediator;

    public IssueIntakePlugin(ConversationData conversationData, ITurnContext<IMessageActivity> turnContext, IMediator mediator)
    {
        _conversationData = conversationData;
        _turnContext = turnContext;
        _mediator = mediator;
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
            // Extract phone number from contact string (assuming format "Name - +1234567890")
            var phoneNumber = ExtractPhoneNumber(contact);
            var reporterName = ExtractReporterName(contact);

            // Create the issue using MediatR command
            var command = new IssueIntakeCommand
            {
                ReporterPhone = phoneNumber,
                ReporterName = reporterName,
                Channel = "WhatsApp",
                Category = category ?? "General",
                Product = product!,
                Severity = severity!,
                Priority = priority!,
                Summary = summary!,
                Description = description!,
                ConsentFlag = true,
                Status = "New",
                Attachments = _conversationData.Attachments?.Select(a => new IssueAttachmentData
                {
                    Name = a.Name,
                    ContentType = a.ContentType,
                    Url = a.Url,
                    Size = 0 // Default size if not available
                }).ToList()
            };

            var result = await _mediator.Send(command);
            
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
                return confirmationMessage + $"\n\n📋 **Issue ID:** {result.Data}";
            }
            else
            {
                return $"There was a problem creating your issue: {result.ErrorMessage}. Please try again or contact support directly.";
            }
        }
        catch (Exception ex)
        {
            return "There was a problem processing your issue. Please try again or contact support directly.";
        }
    }

    [KernelFunction(name: "CheckForDuplicateIssue")]
    [Description("OPTIONAL: Check if a similar issue has been reported recently by the same contact within the last 7 days to prevent duplicate issue creation. Use this before creating a new issue if you suspect the user might have already reported this problem.")]
    public async Task<string> CheckForDuplicateIssueAsync(
        [Description("Phone number of the reporting user in E.164 format (e.g., +27123456789)")] string phoneNumber,
        [Description("Detailed description of the current issue being reported for similarity comparison")] string currentDescription
    )
    {
        // This would integrate with your deduplication service
        // For now, return a simple response
        return "No recent duplicate issues found. Proceeding with new issue creation.";
    }

    private string DetermineSeverity(string description, string category)
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

    private string DeterminePriority(string severity, string category)
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

    private string ExtractPhoneNumber(string contact)
    {
        // Extract phone number from contact string
        // Expected formats: "John Doe - +1234567890", "+1234567890", "John - +1234567890"
        if (string.IsNullOrWhiteSpace(contact))
            return _turnContext.Activity.From.Id; // Fallback to WhatsApp ID
            
        var phonePattern = @"\+?[1-9]\d{1,14}";
        var match = System.Text.RegularExpressions.Regex.Match(contact, phonePattern);
        
        return match.Success ? match.Value : _turnContext.Activity.From.Id;
    }
    
    private string? ExtractReporterName(string contact)
    {
        // Extract name from contact string
        // Expected formats: "John Doe - +1234567890", "John - +1234567890"
        if (string.IsNullOrWhiteSpace(contact))
            return null;
            
        var parts = contact.Split(" - ");
        if (parts.Length > 1)
        {
            return parts[0].Trim();
        }
        
        // If no separator, check if it's just a phone number
        var phonePattern = @"^\+?[1-9]\d{1,14}$";
        if (System.Text.RegularExpressions.Regex.IsMatch(contact.Trim(), phonePattern))
        {
            return null; // It's just a phone number
        }
        
        return contact.Trim(); // Assume the whole string is a name
    }

    private string FormatIssueConfirmation(object issueData)
    {
        var data = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(issueData));

        var message = "✅ **Issue Successfully Collected**\n\n";
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