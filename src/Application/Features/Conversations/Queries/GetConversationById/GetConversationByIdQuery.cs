// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetConversationById;

public record GetConversationByIdQuery(string ConversationId) : IRequest<Result<ConversationDetailsDto>>;

public class GetConversationByIdQueryHandler : IRequestHandler<GetConversationByIdQuery, Result<ConversationDetailsDto>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;
    private readonly ILogger<GetConversationByIdQueryHandler> _logger;

    public GetConversationByIdQueryHandler(
        IApplicationDbContextFactory dbContextFactory,
        IMapper mapper,
        ILogger<GetConversationByIdQueryHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ConversationDetailsDto>> Handle(GetConversationByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

            var conversation = await db.Conversations
                .Include(c => c.Messages)
                .Include(c => c.Participants)
                .Include(c => c.Handoffs)
                .FirstOrDefaultAsync(c => c.ConversationReference == request.ConversationId, cancellationToken);

            if (conversation == null)
            {
                _logger.LogWarning("Conversation with ID {ConversationId} not found", request.ConversationId);
                return await Result<ConversationDetailsDto>.FailureAsync($"Conversation with ID {request.ConversationId} not found");
            }

            var conversationDto = _mapper.Map<ConversationDto>(conversation);
            var messagesDto = _mapper.Map<List<ConversationMessageDto>>(conversation.Messages.OrderBy(m => m.Timestamp));

            // Get agent name if conversation has current agent
            string? currentAgentName = null;
            if (!string.IsNullOrEmpty(conversation.CurrentAgentId))
            {
                var agent = await db.Users
                    .Where(u => u.Id == conversation.CurrentAgentId)
                    .Select(u => u.DisplayName)
                    .FirstOrDefaultAsync(cancellationToken);
                
                currentAgentName = agent ?? "Unknown Agent";
            }

            var conversationDetails = new ConversationDetailsDto
            {
                Conversation = conversationDto with { CurrentAgentName = currentAgentName },
                Messages = messagesDto,
                Participants = conversation.Participants.Select(p => new ConversationParticipantDto
                {
                    Id = p.Id,
                    ConversationId = p.ConversationId,
                    Type = p.Type,
                    ParticipantId = p.ParticipantId,
                    ParticipantName = p.ParticipantName,
                    WhatsAppPhoneNumber = p.WhatsAppPhoneNumber,
                    JoinedAt = p.JoinedAt,
                    LeftAt = p.LeftAt,
                    IsActive = p.IsActive
                }).ToList(),
                HandoffHistory = conversation.Handoffs.OrderByDescending(h => h.InitiatedAt).Select(h => new ConversationHandoffDto
                {
                    Id = h.Id,
                    ConversationId = h.ConversationReference,
                    HandoffType = h.HandoffType,
                    FromParticipantType = h.FromParticipantType,
                    ToParticipantType = h.ToParticipantType,
                    FromAgentId = h.FromAgentId,
                    ToAgentId = h.ToAgentId,
                    Reason = h.Reason,
                    ConversationTranscript = h.ConversationTranscript,
                    ContextData = h.ContextData,
                    Status = h.Status,
                    InitiatedAt = h.InitiatedAt,
                    AcceptedAt = h.AcceptedAt,
                    CompletedAt = h.CompletedAt,
                    Notes = h.Notes
                }).ToList()
            };

            _logger.LogInformation("Retrieved conversation details for {ConversationId} with {MessageCount} messages",
                request.ConversationId, messagesDto.Count);

            return await Result<ConversationDetailsDto>.SuccessAsync(conversationDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation details for ID: {ConversationId}", request.ConversationId);
            return await Result<ConversationDetailsDto>.FailureAsync($"Error retrieving conversation: {ex.Message}");
        }
    }
}
