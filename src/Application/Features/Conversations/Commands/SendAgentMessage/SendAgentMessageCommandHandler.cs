using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;
using CleanArchitecture.Blazor.Application.Common.Models;
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

        // Send agent message via Bot Controller (Bot as Proxy pattern)
        if (!string.IsNullOrEmpty(conversation.ConversationReference))
        {
            try
            {
                await SendAgentMessageToBotController(
                    conversation.ConversationReference, // still pass as ConversationId for legacy
                    request.Content,
                    currentUserId,
                    agent.ApplicationUser.DisplayName ?? agent.ApplicationUser.UserName ?? "Agent",
                    conversation.TenantId,
                    conversation.ConversationReference, // pass full reference as JSON string
                    null, // ServiceUrl will be extracted in BotController
                    cancellationToken);
            }
            catch (Exception ex)
            {
                // Log error but don't fail the entire operation
                // The message is already saved in the database
                Console.WriteLine($"Failed to send agent message via Bot Controller: {ex.Message}");
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

    private async Task SendAgentMessageToBotController(
        string conversationId, 
        string content, 
        string agentId, 
        string agentName, 
        string tenantId, 
        string? conversationReference, // full JSON string
        string? serviceUrl, // not used here
        CancellationToken cancellationToken)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        
        // Retrieve the ConversationChannelData from the Conversation entity
        string? storedConversationReference = null;
        string? storedServiceUrl = null;
        
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
            
            // Get the conversation and its ConversationChannelData
            var conversation = await db.Conversations
                .Where(c => c.ConversationReference == conversationId)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (conversation != null && !string.IsNullOrEmpty(conversation.ConversationChannelData))
            {
                storedConversationReference = conversation.ConversationChannelData;
                
                // Try to extract ServiceUrl from the stored ConversationReference
                try
                {
                    using var jsonDoc = JsonDocument.Parse(storedConversationReference);
                    if (jsonDoc.RootElement.TryGetProperty("ServiceUrl", out var serviceUrlElement))
                    {
                        storedServiceUrl = serviceUrlElement.GetString();
                    }
                }
                catch { }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to retrieve stored ConversationChannelData: {ex.Message}");
        }
        
        var request = new
        {
            ConversationId = conversationId,
            Content = content,
            AgentId = agentId,
            AgentName = agentName,
            TenantId = tenantId,
            Timestamp = DateTime.UtcNow,
            ConversationReference = storedConversationReference ?? conversationReference,
            ServiceUrl = storedServiceUrl ?? serviceUrl
        };

        var json = JsonSerializer.Serialize(request);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        // Send to Bot Controller's agent activity endpoint
        var botServiceUrl = "http://localhost:3978/api/agent-activity";
        
        var response = await httpClient.PostAsync(botServiceUrl, httpContent, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Bot Controller returned error: {response.StatusCode} - {errorContent}");
        }
    }
}