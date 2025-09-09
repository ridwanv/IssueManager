// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using FluentValidation;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Commands.TransferConversation;

public class TransferConversationCommandValidator : AbstractValidator<TransferConversationCommand>
{
    public TransferConversationCommandValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty()
            .WithMessage("Conversation ID is required");
            
        RuleFor(x => x.ToAgentId)
            .NotEmpty()
            .WithMessage("Target agent ID is required");
            
        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .WithMessage("Transfer reason cannot exceed 500 characters");
    }
}