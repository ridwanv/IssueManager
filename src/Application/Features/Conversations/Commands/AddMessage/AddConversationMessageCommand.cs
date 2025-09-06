// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;
using CleanArchitecture.Blazor.Domain.Entities;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Commands.AddMessage;

public record AddConversationMessageCommand(
    ConversationMessageCreateDto Message
) : IRequest<Result<int>>;

public class AddConversationMessageCommandHandler : IRequestHandler<AddConversationMessageCommand, Result<int>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;

    public AddConversationMessageCommandHandler(
        IApplicationDbContextFactory dbContextFactory,
        IMapper mapper)
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
    }

    public async Task<Result<int>> Handle(AddConversationMessageCommand request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

        // Get or create conversation record
        var conversation = await db.Conversations
            .FirstOrDefaultAsync(c => c.ConversationReference == request.Message.BotFrameworkConversationId, cancellationToken);

        if (conversation == null)
        {
            // Create new conversation record
            conversation = new Conversation
            {
                ConversationReference = request.Message.BotFrameworkConversationId,
                Status = Domain.Enums.ConversationStatus.Active,
                Mode = Domain.Enums.ConversationMode.Bot,
                LastActivityAt = DateTime.UtcNow,
                TenantId = "default", // Use default tenant for bot conversations
                MessageCount = 0
            };
            
            db.Conversations.Add(conversation);
            await db.SaveChangesAsync(cancellationToken); // Save to get the ID
        }

        // Map the message
        var message = _mapper.Map<ConversationMessage>(request.Message);
        message.ConversationId = conversation.Id;
        message.TenantId = conversation.TenantId;
        message.IsEscalated = conversation.IsEscalated;

        // Add the message
        db.ConversationMessages.Add(message);

        // Update conversation metadata
        conversation.MessageCount++;
        conversation.LastActivityAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return await Result<int>.SuccessAsync(message.Id);
    }
}
