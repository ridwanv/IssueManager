using AutoMapper;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Models;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetIncrementalMessages;

public class GetIncrementalMessagesQueryHandler : IRequestHandler<GetIncrementalMessagesQuery, Result<List<ConversationMessageDto>>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;

    public GetIncrementalMessagesQueryHandler(IApplicationDbContextFactory dbContextFactory, IMapper mapper)
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
    }

    public async Task<Result<List<ConversationMessageDto>>> Handle(GetIncrementalMessagesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

            if (!int.TryParse(request.ConversationId, out var conversationId))
            {
                return Result<List<ConversationMessageDto>>.Failure("Invalid conversation ID format.");
            }

            var query = db.ConversationMessages
                .Where(m => m.ConversationId == conversationId);

            if (request.LastMessageTimestamp.HasValue)
            {
                query = query.Where(m => m.Timestamp > request.LastMessageTimestamp.Value);
            }

            var messages = await query
                .OrderBy(m => m.Timestamp)
                .ToListAsync(cancellationToken);

            var messageDtos = _mapper.Map<List<ConversationMessageDto>>(messages);
            return Result<List<ConversationMessageDto>>.Success(messageDtos);
        }
        catch (Exception ex)
        {
            return Result<List<ConversationMessageDto>>.Failure($"Error retrieving incremental messages: {ex.Message}");
        }
    }
}