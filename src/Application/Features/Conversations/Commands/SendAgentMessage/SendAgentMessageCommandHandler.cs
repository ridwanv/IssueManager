using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Commands.SendAgentMessage;

public class SendAgentMessageCommandHandler : IRequestHandler<SendAgentMessageCommand, Result<Unit>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IUserContextAccessor _userContextAccessor;
    private readonly IApplicationHubWrapper _hubWrapper;
    private readonly IHttpClientFactory _httpClientFactory;

    public SendAgentMessageCommandHandler(
        IApplicationDbContextFactory dbContextFactory,
        IUserContextAccessor userContextAccessor,
        IApplicationHubWrapper hubWrapper,
        IHttpClientFactory httpClientFactory)
    {
        _dbContextFactory = dbContextFactory;
        _userContextAccessor = userContextAccessor;
        _hubWrapper = hubWrapper;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Result<Unit>> Handle(SendAgentMessageCommand request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
        
        var currentUserId = _userContextAccessor.Current?.UserId;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Result<Unit>.Failure("User not authenticated.");
        }

        // Find the conversation and validate agent assignment
        var conversation = await db.Conversations
            .Include(c => c.Messages)
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.ConversationReference == request.ConversationId, cancellationToken);

        if (conversation == null)
        {
            return Result<Unit>.Failure("Conversation not found.");
        }

        // Verify this agent is assigned to this conversation
        if (conversation.CurrentAgentId != currentUserId)
        {
            return Result<Unit>.Failure("You are not assigned to this conversation.");
        }

        // Get agent details
        var agent = await db.Agents
            .Include(a => a.ApplicationUser)
            .FirstOrDefaultAsync(a => a.ApplicationUserId == currentUserId, cancellationToken);

        if (agent == null)
        {
            return Result<Unit>.Failure("Agent profile not found.");
        }

        // Create the message
        var message = new ConversationMessage
        {
            ConversationId = conversation.Id,
            BotFrameworkConversationId = conversation.ConversationReference,
            Content = request.Content,
            Role = "agent",
            UserName = agent.ApplicationUser.DisplayName ?? agent.ApplicationUser.UserName ?? "Agent",
            Timestamp = DateTime.UtcNow,
            TenantId = conversation.TenantId
        };

        db.ConversationMessages.Add(message);
        
        // Update conversation last activity
        conversation.LastActivityAt = DateTime.UtcNow;
        
        await db.SaveChangesAsync(cancellationToken);

        // Send message via WhatsApp through Bot service
        var customerParticipant = conversation.Participants
            .FirstOrDefault(p => p.Type == ParticipantType.Customer);
            
        if (customerParticipant != null && !string.IsNullOrEmpty(customerParticipant.WhatsAppPhoneNumber))
        {
            try
            {
                await SendMessageToBotService(conversation.Id, customerParticipant.WhatsAppPhoneNumber, 
                    request.Content, agent.ApplicationUser.DisplayName ?? "Agent");
            }
            catch (Exception ex)
            {
                // Log error but don't fail the entire operation
                // The message is already saved in the database
                Console.WriteLine($"Failed to send WhatsApp message: {ex.Message}");
            }
        }

        // Broadcast the new message to all clients
        await _hubWrapper.BroadcastNewConversationMessage(
            conversation.ConversationReference, 
            message.UserName, 
            message.Content, 
            true);

        return await Result<Unit>.SuccessAsync(Unit.Value);
    }

    private async Task SendMessageToBotService(int conversationId, string phoneNumber, string message, string agentName)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        
        var request = new
        {
            ConversationId = conversationId,
            PhoneNumber = phoneNumber,
            Message = message,
            AgentName = agentName
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // This should point to the Bot service endpoint
        // In production, this URL should come from configuration
        var botServiceUrl = "http://localhost:5000/api/agent-message"; // TODO: Move to config
        
        var response = await httpClient.PostAsync(botServiceUrl, content);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Bot service returned error: {response.StatusCode} - {errorContent}");
        }
    }
}