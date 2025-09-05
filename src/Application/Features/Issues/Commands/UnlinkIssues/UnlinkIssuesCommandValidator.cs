// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FluentValidation;

namespace CleanArchitecture.Blazor.Application.Features.Issues.Commands.UnlinkIssues;

public class UnlinkIssuesCommandValidator : AbstractValidator<UnlinkIssuesCommand>
{
    public UnlinkIssuesCommandValidator()
    {
        RuleFor(x => x.IssueLinkId)
            .NotEmpty()
            .WithMessage("Issue Link ID is required");
            
        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .WithMessage("Reason cannot exceed 500 characters");
    }
}