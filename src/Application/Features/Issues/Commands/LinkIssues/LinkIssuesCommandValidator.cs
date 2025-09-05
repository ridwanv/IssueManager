// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FluentValidation;

namespace CleanArchitecture.Blazor.Application.Features.Issues.Commands.LinkIssues;

public class LinkIssuesCommandValidator : AbstractValidator<LinkIssuesCommand>
{
    public LinkIssuesCommandValidator()
    {
        RuleFor(x => x.ParentIssueId)
            .NotEmpty()
            .WithMessage("Parent Issue ID is required");
            
        RuleFor(x => x.ChildIssueId)
            .NotEmpty()
            .WithMessage("Child Issue ID is required");
            
        RuleFor(x => x)
            .Must(x => x.ParentIssueId != x.ChildIssueId)
            .WithMessage("Cannot link an issue to itself")
            .WithName("Issue Self-Link");
            
        RuleFor(x => x.LinkType)
            .IsInEnum()
            .WithMessage("Invalid link type specified");
            
        RuleFor(x => x.ConfidenceScore)
            .InclusiveBetween(0.0m, 1.0m)
            .When(x => x.ConfidenceScore.HasValue)
            .WithMessage("Confidence score must be between 0.0 and 1.0");
            
        RuleFor(x => x.Metadata)
            .MaximumLength(1000)
            .WithMessage("Metadata cannot exceed 1000 characters");
            
        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .WithMessage("Reason cannot exceed 500 characters");
    }
}