// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Features.Agents.DTOs;
using FluentValidation;

namespace CleanArchitecture.Blazor.Server.UI.Pages.Agents.Components;

public class ConvertUserToAgentDtoValidator : AbstractValidator<ConvertUserToAgentDto>
{
    public ConvertUserToAgentDtoValidator()
    {
        RuleFor(x => x.MaxConcurrentConversations)
            .GreaterThan(0)
            .LessThanOrEqualTo(50)
            .WithMessage("Max concurrent conversations must be between 1 and 50");

        RuleFor(x => x.Priority)
            .GreaterThan(0)
            .LessThanOrEqualTo(10)
            .WithMessage("Priority must be between 1 and 10");

        RuleFor(x => x.Skills)
            .MaximumLength(1000)
            .WithMessage("Skills must not exceed 1000 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Notes must not exceed 2000 characters");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<ConvertUserToAgentDto>.CreateWithOptions((ConvertUserToAgentDto)model, x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}
