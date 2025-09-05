// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetMessages;

public record GetConversationMessagesQuery(
    string BotFrameworkConversationId,
    int? Limit = null,
    DateTime? Since = null
) : IRequest<Result<List<ConversationMessageDto>>>;

public class GetConversationMessagesQueryHandler : IRequestHandler<GetConversationMessagesQuery, Result<List<ConversationMessageDto>>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;

    public GetConversationMessagesQueryHandler(
        IApplicationDbContextFactory dbContextFactory,
        IMapper mapper)
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
    }

    public async Task<Result<List<ConversationMessageDto>>> Handle(GetConversationMessagesQuery request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

        var query = db.ConversationMessages
            .Where(m => m.BotFrameworkConversationId == request.BotFrameworkConversationId);

        if (request.Since.HasValue)
        {
            query = query.Where(m => m.Timestamp >= request.Since.Value);
        }

        query = query.OrderBy(m => m.Timestamp);

        if (request.Limit.HasValue)
        {
            query = query.Take(request.Limit.Value);
        }

        var messages = await query.ToListAsync(cancellationToken);
        var messagesDto = _mapper.Map<List<ConversationMessageDto>>(messages);

        return await Result<List<ConversationMessageDto>>.SuccessAsync(messagesDto);
    }
}
