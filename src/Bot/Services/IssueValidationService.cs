using System.Text.RegularExpressions;

namespace IssueManager.Bot.Services;

public class IssueValidationService
{
    private readonly List<string> _validCategories = new()
    {
        "Technical", "Policy", "Claims", "Billing", "Account", "General"
    };
    
    private readonly List<string> _validSeverityLevels = new()
    {
        "Critical", "High", "Medium", "Low"
    };
    
    private readonly List<string> _validPriorityLevels = new()
    {
        "Urgent", "High", "Medium", "Low"
    };

    public ValidationResult ValidateContact(string contact)
    {
        if (string.IsNullOrWhiteSpace(contact))
        {
            return ValidationResult.Invalid("Contact information is required. Please provide your name and phone number.");
        }

        // Basic phone number validation (E.164 format or common formats)
        var phonePattern = @"(\+\d{1,3}[-.\s]?)?\(?\d{1,4}\)?[-.\s]?\d{1,4}[-.\s]?\d{1,9}";
        var hasPhone = Regex.IsMatch(contact, phonePattern);
        
        if (!hasPhone)
        {
            return ValidationResult.Invalid("Please include your phone number in your contact information (e.g., 'John Smith +27123456789').");
        }
        
        // Check for name (at least 2 words with letters)
        var namePattern = @"[A-Za-z]{2,}\s+[A-Za-z]{2,}";
        var hasName = Regex.IsMatch(contact, namePattern);
        
        if (!hasName)
        {
            return ValidationResult.Invalid("Please include both your first and last name along with your phone number (e.g., 'John Smith +27123456789').");
        }

        return ValidationResult.Valid();
    }

    public ValidationResult ValidateProduct(string product)
    {
        if (string.IsNullOrWhiteSpace(product))
        {
            return ValidationResult.Invalid("Product or system name is required. Please specify which system you're having issues with.");
        }

        if (product.Length < 3)
        {
            return ValidationResult.Invalid("Please provide a more descriptive product or system name (e.g., 'Mobile App', 'Website', 'Policy System').");
        }

        return ValidationResult.Valid();
    }

    public ValidationResult ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return ValidationResult.Invalid("Issue description is required. Please describe what happened and what you were trying to do.");
        }

        if (description.Length < 10)
        {
            return ValidationResult.Invalid("Please provide a more detailed description of the issue. Include what you were trying to do and what went wrong.");
        }

        return ValidationResult.Valid();
    }

    public ValidationResult ValidateCategory(string category)
    {
        if (!string.IsNullOrWhiteSpace(category) && !_validCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
        {
            return ValidationResult.Invalid($"Invalid category. Please choose from: {string.Join(", ", _validCategories)}");
        }

        return ValidationResult.Valid();
    }

    public ValidationResult ValidateSeverity(string severity)
    {
        if (!string.IsNullOrWhiteSpace(severity) && !_validSeverityLevels.Contains(severity, StringComparer.OrdinalIgnoreCase))
        {
            return ValidationResult.Invalid($"Invalid severity level. Please choose from: {string.Join(", ", _validSeverityLevels)}");
        }

        return ValidationResult.Valid();
    }

    public ValidationResult ValidatePriority(string priority)
    {
        if (!string.IsNullOrWhiteSpace(priority) && !_validPriorityLevels.Contains(priority, StringComparer.OrdinalIgnoreCase))
        {
            return ValidationResult.Invalid($"Invalid priority level. Please choose from: {string.Join(", ", _validPriorityLevels)}");
        }

        return ValidationResult.Valid();
    }

    public string GetRetryPrompt(string fieldName, ValidationResult validationResult, int retryCount)
    {
        var baseMessage = validationResult.ErrorMessage;
        
        if (retryCount >= 3)
        {
            return $"🔄 **Multiple Validation Attempts**\n\n" +
                   $"{baseMessage}\n\n" +
                   $"This is your {retryCount} attempt. If you continue having trouble, " +
                   $"you can type 'help' for more guidance or start over with a fresh conversation.";
        }

        return $"❌ **Validation Error**\n\n{baseMessage}";
    }

    public string GetHelpMessage(string fieldName)
    {
        return fieldName.ToLowerInvariant() switch
        {
            "contact" => "📞 **Contact Help**\n" +
                        "Please provide your full name and phone number in this format:\n" +
                        "• John Smith +27123456789\n" +
                        "• Mary Johnson 0821234567\n" +
                        "• David Brown +1-555-123-4567",
            
            "product" => "📱 **Product Help**\n" +
                        "Please specify which system or product you're having issues with:\n" +
                        "• Mobile App\n• Website\n• Policy System\n• Claims Portal\n• Billing System\n• Customer Portal",
            
            "description" => "📝 **Description Help**\n" +
                           "Please provide a detailed description that includes:\n" +
                           "• What were you trying to do?\n" +
                           "• What happened instead?\n" +
                           "• Any error messages you saw\n" +
                           "• When did this happen?\n" +
                           "• Does it happen every time?",
            
            "category" => $"📂 **Category Help**\n" +
                         $"Please choose from these categories:\n" +
                         $"• {string.Join("\n• ", _validCategories)}",
            
            _ => "ℹ️ **General Help**\n" +
                "I'm here to help you report your issue step by step. " +
                "Please answer each question as clearly as possible. " +
                "If you get stuck, you can type 'start over' to begin fresh."
        };
    }

    public bool IsUnrecognizedResponse(string response, string expectedField)
    {
        if (string.IsNullOrWhiteSpace(response))
            return true;

        var responseWords = response.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        // Too short responses are likely unclear
        if (responseWords.Length == 1 && response.Length < 3)
            return true;

        // Common unclear responses
        var unclearResponses = new[] { "yes", "no", "ok", "idk", "dunno", "maybe", "?", "???" };
        if (unclearResponses.Contains(response.ToLowerInvariant().Trim()))
            return true;

        return false;
    }

    public string GetFallbackResponse(string response, string expectedField)
    {
        return $"🤔 I'm not sure I understand your response: \"{response}\"\n\n" +
               $"I was expecting information about {expectedField}. " +
               $"Could you please provide more details?\n\n" +
               $"Type 'help' if you need guidance on what to provide.";
    }
}

public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string ErrorMessage { get; private set; }

    private ValidationResult(bool isValid, string errorMessage = "")
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Valid() => new(true);
    public static ValidationResult Invalid(string errorMessage) => new(false, errorMessage);
}