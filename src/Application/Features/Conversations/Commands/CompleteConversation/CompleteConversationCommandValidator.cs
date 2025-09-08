// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Enums;
using FluentValidation;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Commands.CompleteConversation;

public class CompleteConversationCommandValidator : AbstractValidator<CompleteConversationCommand>
{
    public CompleteConversationCommandValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("Conversation ID is required.");

        RuleFor(x => x.Category)
            .IsInEnum()
            .WithMessage("Please select a valid resolution category.");

        RuleFor(x => x.ResolutionNotes)
            .NotEmpty()
            .WithMessage("Resolution notes are required.")
            .MinimumLength(20)
            .WithMessage("Resolution notes must be at least 20 characters long.")
            .MaximumLength(2000)
            .WithMessage("Resolution notes cannot exceed 2000 characters.");
    }
}