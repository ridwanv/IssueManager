using System.Text.RegularExpressions;

namespace CleanArchitecture.Blazor.Application.Features.Issues.Commands.Create;

public class IssueIntakeCommandValidator : AbstractValidator<IssueIntakeCommand>
{
    public IssueIntakeCommandValidator()
    {
        RuleFor(v => v.ReporterPhone)
            .NotEmpty()
            .MaximumLength(20)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Phone number must be in E.164 format (e.g., +1234567890)");
            
        RuleFor(v => v.ReporterName)
            .MaximumLength(100)
            .When(v => !string.IsNullOrWhiteSpace(v.ReporterName));
            
        RuleFor(v => v.Channel)
            .NotEmpty()
            .MaximumLength(50);
            
        RuleFor(v => v.Category)
            .NotEmpty()
            .Must(BeValidCategory)
            .WithMessage("Category must be one of: Technical, Policy, Claims, Billing, Account, General");
            
        RuleFor(v => v.Product)
            .NotEmpty()
            .MaximumLength(100);
            
        RuleFor(v => v.Severity)
            .NotEmpty()
            .Must(BeValidSeverity)
            .WithMessage("Severity must be one of: Critical, High, Medium, Low");
            
        RuleFor(v => v.Priority)
            .NotEmpty()
            .Must(BeValidPriority)
            .WithMessage("Priority must be one of: Urgent, High, Medium, Low");
            
        RuleFor(v => v.Summary)
            .NotEmpty()
            .MaximumLength(200);
            
        RuleFor(v => v.Description)
            .NotEmpty()
            .MaximumLength(2000);
            
        RuleFor(v => v.Status)
            .NotEmpty()
            .MaximumLength(50);
    }
    
    private bool BeValidCategory(string category)
    {
        var validCategories = new[] { "Technical", "Policy", "Claims", "Billing", "Account", "General" };
        return validCategories.Contains(category);
    }
    
    private bool BeValidSeverity(string severity)
    {
        var validSeverities = new[] { "Critical", "High", "Medium", "Low" };
        return validSeverities.Contains(severity);
    }
    
    private bool BeValidPriority(string priority)
    {
        var validPriorities = new[] { "Urgent", "High", "Medium", "Low" };
        return validPriorities.Contains(priority);
    }
}