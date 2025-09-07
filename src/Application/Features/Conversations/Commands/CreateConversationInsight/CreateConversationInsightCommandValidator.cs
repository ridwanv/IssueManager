// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Commands.CreateConversationInsight;

/// <summary>
/// Validator for CreateConversationInsightCommand
/// </summary>
public class CreateConversationInsightCommandValidator : AbstractValidator<CreateConversationInsightCommand>
{
    public CreateConversationInsightCommandValidator()
    {
        RuleFor(v => v.ConversationId)
            .GreaterThan(0)
            .WithMessage("ConversationId must be a positive integer.");

        RuleFor(v => v.SentimentScore)
            .InclusiveBetween(-1.0m, 1.0m)
            .WithMessage("SentimentScore must be between -1.0 and 1.0.");

        RuleFor(v => v.SentimentLabel)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("SentimentLabel is required and must not exceed 50 characters.");

        RuleFor(v => v.KeyThemes)
            .Must(themes => themes.Count <= 10)
            .WithMessage("KeyThemes cannot exceed 10 items.")
            .Must(themes => themes.All(theme => !string.IsNullOrWhiteSpace(theme) && theme.Length <= 100))
            .WithMessage("Each KeyTheme must be non-empty and not exceed 100 characters.");

        RuleFor(v => v.CustomerSatisfactionIndicators)
            .Must(indicators => indicators.Count <= 10)
            .WithMessage("CustomerSatisfactionIndicators cannot exceed 10 items.")
            .Must(indicators => indicators.All(indicator => !string.IsNullOrWhiteSpace(indicator) && indicator.Length <= 200))
            .WithMessage("Each CustomerSatisfactionIndicator must be non-empty and not exceed 200 characters.");

        RuleFor(v => v.Recommendations)
            .Must(recommendations => recommendations.Count <= 10)
            .WithMessage("Recommendations cannot exceed 10 items.")
            .Must(recommendations => recommendations.All(rec => !string.IsNullOrWhiteSpace(rec) && rec.Length <= 500))
            .WithMessage("Each Recommendation must be non-empty and not exceed 500 characters.");

        RuleFor(v => v.ProcessingModel)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("ProcessingModel is required and must not exceed 50 characters.");

        RuleFor(v => v.ProcessedAt)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
            .WithMessage("ProcessedAt cannot be more than 5 minutes in the future.");

        RuleFor(v => v.ProcessingDuration)
            .GreaterThanOrEqualTo(TimeSpan.Zero)
            .LessThan(TimeSpan.FromHours(1))
            .WithMessage("ProcessingDuration must be between 0 and 1 hour.");

        RuleFor(v => v.Warnings)
            .Must(warnings => warnings.Count <= 20)
            .WithMessage("Warnings cannot exceed 20 items.")
            .Must(warnings => warnings.All(warning => !string.IsNullOrWhiteSpace(warning) && warning.Length <= 1000))
            .WithMessage("Each Warning must be non-empty and not exceed 1000 characters.");
    }
}