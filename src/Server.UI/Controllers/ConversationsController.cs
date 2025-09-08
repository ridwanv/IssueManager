using Microsoft.AspNetCore.Mvc;
using MediatR;
using CleanArchitecture.Blazor.Application.Features.Conversations.Commands.EscalateConversation;
using CleanArchitecture.Blazor.Application.Features.Conversations.Commands.AssignAgent;
using CleanArchitecture.Blazor.Application.Features.Conversations.Commands.CompleteConversation;
using CleanArchitecture.Blazor.Application.Features.Conversations.Commands.AddMessage;
using CleanArchitecture.Blazor.Domain.Enums;
using CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetEscalatedConversations;
using CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetMessages;
using CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetAllConversations;
using CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetConversationById;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;
using CleanArchitecture.Blazor.Application.Common.Models;
using Microsoft.AspNetCore.SignalR;
using CleanArchitecture.Blazor.Server.UI.Hubs;

namespace CleanArchitecture.Blazor.Server.UI.Controllers;

[ApiController]
[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
public class ConversationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ConversationsController> _logger;
    private readonly IHubContext<ServerHub, ISignalRHub> _hubContext;

    public ConversationsController(
        IMediator mediator, 
        ILogger<ConversationsController> logger,
        IHubContext<ServerHub, ISignalRHub> hubContext)
    {
        _mediator = mediator;
        _logger = logger;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Get all conversations with optional filtering and pagination
    /// Used by the dashboard to display conversation lists
    /// </summary>
    /// <param name="status">Filter by conversation status</param>
    /// <param name="userId">Filter by user ID</param>
    /// <param name="searchTerm">Search term for filtering</param>
    /// <param name="startDate">Filter by start date</param>
    /// <param name="endDate">Filter by end date</param>
    /// <param name="page">Page number for pagination</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="sortBy">Sort field</param>
    /// <param name="sortDescending">Sort direction</param>
    /// <returns>List of conversation summaries</returns>
    [HttpGet]
    public async Task<ActionResult<Result<PaginatedData<ConversationDto>>>> GetAllConversations(
        [FromQuery] string? status = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "LastActivityAt",
        [FromQuery] bool sortDescending = true)
    {
        try
        {
            var query = new GetAllConversationsQuery(
                status, userId, searchTerm, startDate, endDate, 
                page, pageSize, sortBy, sortDescending);
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversations");
            return StatusCode(500, Result<PaginatedData<ConversationDto>>.Failure("An error occurred while retrieving conversations"));
        }
    }

    /// <summary>
    /// Get a specific conversation by Bot Framework conversation ID
    /// Used by agents and bots to retrieve conversation details
    /// </summary>
    /// <param name="conversationId">Bot Framework conversation ID</param>
    /// <returns>Conversation details</returns>
    [HttpGet("{conversationId}")]
    public async Task<ActionResult<Result<ConversationDetailsDto>>> GetConversation(string conversationId)
    {
        try
        {
            var query = new GetConversationByIdQuery(conversationId);
            var result = await _mediator.Send(query);
            
            if (!result.Succeeded)
            {
                return NotFound(result);
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation {ConversationId}", conversationId);
            return StatusCode(500, Result<ConversationDetailsDto>.Failure("An error occurred while retrieving the conversation"));
        }
    }

    /// <summary>
    /// Get conversation statistics
    /// Used by the dashboard to display metrics
    /// </summary>
    /// <returns>Conversation statistics</returns>
    [HttpGet("stats")]
    public Task<ActionResult<Result<ConversationStatsDto>>> GetConversationStats()
    {
        try
        {
            // For now, return basic stats - will create proper query handler later
            var stats = new ConversationStatsDto
            {
                TotalConversations = 0,
                ActiveConversations = 0,
                CompletedConversations = 0,
                EscalatedConversations = 0,
                ConversationsToday = 0,
                ConversationsThisWeek = 0,
                ConversationsThisMonth = 0,
                AverageConversationLength = 0,
                ConversationsWithAttachments = 0
            };

            var result = Result<ConversationStatsDto>.Success(stats);
            return Task.FromResult<ActionResult<Result<ConversationStatsDto>>>(Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation statistics");
            return Task.FromResult<ActionResult<Result<ConversationStatsDto>>>(StatusCode(500, "An error occurred while retrieving statistics"));
        }
    }

    /// <summary>
    /// Update conversation status
    /// Used by agents to change conversation status
    /// </summary>
    /// <param name="conversationId">Bot Framework conversation ID</param>
    /// <param name="request">Status update request</param>
    /// <returns>Success result</returns>
    [HttpPut("{conversationId}/status")]
    public Task<ActionResult<Result>> UpdateConversationStatus(string conversationId, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            // For now, return success - will create proper command handler later
            var result = Result.Success();
            return Task.FromResult<ActionResult<Result>>(Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating conversation status for {ConversationId}", conversationId);
            return Task.FromResult<ActionResult<Result>>(StatusCode(500, "An error occurred while updating conversation status"));
        }
    }

    /// <summary>
    /// Delete a conversation
    /// Used by administrators to remove conversations
    /// </summary>
    /// <param name="conversationId">Bot Framework conversation ID</param>
    /// <returns>Success result</returns>
    [HttpDelete("{conversationId}")]
    public Task<ActionResult<Result>> DeleteConversation(string conversationId)
    {
        try
        {
            // For now, return success - will create proper command handler later
            var result = Result.Success();
            return Task.FromResult<ActionResult<Result>>(Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting conversation {ConversationId}", conversationId);
            return Task.FromResult<ActionResult<Result>>(StatusCode(500, "An error occurred while deleting the conversation"));
        }
    }

    /// <summary>
    /// Get conversation analysis for insights
    /// Used by the dashboard to display conversation insights
    /// </summary>
    /// <param name="conversationId">Bot Framework conversation ID</param>
    /// <returns>Conversation analysis data</returns>
    [HttpGet("{conversationId}/analysis")]
    public Task<ActionResult<Result<ConversationAnalysisDto>>> GetConversationAnalysis(string conversationId)
    {
        try
        {
            // For now, return basic analysis - will create proper query handler later
            var analysis = new ConversationAnalysisDto
            {
                ConversationId = conversationId,
                Sentiment = "Neutral",
                Topics = new List<string>(),
                IntentAnalysis = "No analysis available",
                SatisfactionScore = 0
            };

            var result = Result<ConversationAnalysisDto>.Success(analysis);
            return Task.FromResult<ActionResult<Result<ConversationAnalysisDto>>>(Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing conversation {ConversationId}", conversationId);
            return Task.FromResult<ActionResult<Result<ConversationAnalysisDto>>>(StatusCode(500, "An error occurred while analyzing the conversation"));
        }
    }

    /// <summary>
    /// Perform bulk analysis on conversations
    /// Used by administrators for insights across multiple conversations
    /// </summary>
    /// <returns>Bulk analysis results</returns>
    [HttpPost("bulk-analysis")]
    public Task<ActionResult<Result<ConversationsBulkAnalysisDto>>> GetBulkAnalysis()
    {
        try
        {
            // For now, return basic bulk analysis - will create proper query handler later
            var bulkAnalysis = new ConversationsBulkAnalysisDto
            {
                TotalAnalyzed = 0,
                CommonTopics = new List<string>(),
                OverallSentiment = "Neutral",
                AverageSatisfactionScore = 0,
                AnalysisTimestamp = DateTime.UtcNow
            };

            var result = Result<ConversationsBulkAnalysisDto>.Success(bulkAnalysis);
            return Task.FromResult<ActionResult<Result<ConversationsBulkAnalysisDto>>>(Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk analysis");
            return Task.FromResult<ActionResult<Result<ConversationsBulkAnalysisDto>>>(StatusCode(500, "An error occurred while performing bulk analysis"));
        }
    }

    /// <summary>
    /// Get user intent insights across conversations
    /// Used by the dashboard for user behavior analysis
    /// </summary>
    /// <returns>User intent insights</returns>
    [HttpGet("insights")]
    public Task<ActionResult<Result<UserIntentInsightsDto>>> GetUserIntentInsights()
    {
        try
        {
            // For now, return basic insights - will create proper query handler later
            var insights = new UserIntentInsightsDto
            {
                CommonIntents = new List<string>(),
                IntentDistribution = new Dictionary<string, int>(),
                TrendingTopics = new List<string>(),
                AnalysisTimestamp = DateTime.UtcNow
            };

            var result = Result<UserIntentInsightsDto>.Success(insights);
            return Task.FromResult<ActionResult<Result<UserIntentInsightsDto>>>(Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user intent insights");
            return Task.FromResult<ActionResult<Result<UserIntentInsightsDto>>>(StatusCode(500, "An error occurred while retrieving user intent insights"));
        }
    }

    /// <summary>
    /// Escalate a conversation to human agent
    /// Used by the WhatsApp bot to escalate conversations
    /// </summary>
    /// <param name="request">Escalation request details</param>
    /// <returns>Created conversation ID</returns>
    [HttpPost("escalate")]
    public async Task<ActionResult<Result<int>>> EscalateConversation([FromBody] EscalateConversationRequest request)
    {
        try
        {
            var command = new EscalateConversationCommand(
                request.ConversationId,
                request.Reason,
                request.ConversationTranscript,
                request.WhatsAppPhoneNumber
            );

            var result = await _mediator.Send(command);
            
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            _logger.LogInformation("Conversation {ConversationId} escalated successfully. Database ID: {DbId}", 
                request.ConversationId, result.Data);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error escalating conversation {ConversationId}", request.ConversationId);
            return StatusCode(500, "An error occurred while escalating the conversation");
        }
    }

    /// <summary>
    /// Get all escalated conversations
    /// Used by the Agent Dashboard
    /// </summary>
    /// <returns>List of escalated conversations</returns>
    [HttpGet("escalated")]
    public async Task<ActionResult<Result<List<ConversationDto>>>> GetEscalatedConversations()
    {
        try
        {
            var query = new GetEscalatedConversationsQuery();
            var result = await _mediator.Send(query);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving escalated conversations");
            return StatusCode(500, "An error occurred while retrieving escalated conversations");
        }
    }

    /// <summary>
    /// Assign an agent to a conversation
    /// Used by agents to accept escalated conversations
    /// </summary>
    /// <param name="conversationId">Database conversation ID</param>
    /// <param name="request">Assignment request</param>
    /// <returns>Success result</returns>
    [HttpPost("{conversationId:int}/assign")]
    public async Task<ActionResult<Result>> AssignAgent(string conversationId, [FromBody] AssignAgentRequest request)
    {
        try
        {
            var command = new AssignAgentCommand(conversationId, request.AgentId);
            var result = await _mediator.Send(command);
            
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning agent to conversation {ConversationId}", conversationId);
            return StatusCode(500, "An error occurred while assigning the agent");
        }
    }

    /// <summary>
    /// Complete a conversation
    /// Used by agents to mark conversations as completed
    /// </summary>
    /// <param name="conversationId">Database conversation ID</param>
    /// <param name="request">Completion request</param>
    /// <returns>Success result</returns>
    [HttpPost("{conversationId:int}/complete")]
    public async Task<ActionResult<Result>> CompleteConversation(string conversationId, [FromBody] CompleteConversationRequest request)
    {
        try
        {
            var command = new CompleteConversationCommand(conversationId, ResolutionCategory.Resolved, request.Summary ?? "Completed via API", true);
            var result = await _mediator.Send(command);
            
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing conversation {ConversationId}", conversationId);
            return StatusCode(500, "An error occurred while completing the conversation");
        }
    }

    /// <summary>
    /// Add a message to a conversation
    /// Used by the bot to store conversation messages
    /// </summary>
    /// <param name="request">Message details</param>
    /// <returns>Created message ID</returns>
    [HttpPost("messages")]
    public async Task<ActionResult<Result<int>>> AddMessage([FromBody] ConversationMessageCreateDto request)
    {
        try
        {
            var command = new AddConversationMessageCommand(request);
            var result = await _mediator.Send(command);
            
            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            _logger.LogInformation("Message added to conversation {ConversationId}. Message ID: {MessageId}", 
                request.BotFrameworkConversationId, result.Data);

            // Broadcast real-time notification so global NotificationIndicator & other listeners update
            try
            {
                var fromName = request.UserName ?? request.UserId ?? (request.Role == "agent" ? "Agent" : "User");
                var content = request.Content ?? string.Empty;
                var isFromAgent = string.Equals(request.Role, "agent", StringComparison.OrdinalIgnoreCase);
                await _hubContext.Clients.All.NewConversationMessage(
                    request.BotFrameworkConversationId,
                    fromName,
                    content,
                    isFromAgent
                );
                _logger.LogInformation("Broadcasted NewConversationMessage for {ConversationId}", request.BotFrameworkConversationId);
            }
            catch (Exception broadcastEx)
            {
                _logger.LogError(broadcastEx, "Failed to broadcast NewConversationMessage for {ConversationId}", request.BotFrameworkConversationId);
                // Do not fail the API on broadcast errors
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding message to conversation {ConversationId}", request.BotFrameworkConversationId);
            return StatusCode(500, "An error occurred while adding the message");
        }
    }

    /// <summary>
    /// Get messages for a conversation
    /// Used by agents and the bot to retrieve conversation history
    /// </summary>
    /// <param name="conversationId">Bot Framework conversation ID</param>
    /// <param name="limit">Maximum number of messages to return</param>
    /// <param name="since">Only return messages after this timestamp</param>
    /// <returns>List of conversation messages</returns>
    [HttpGet("messages/{conversationId}")]
    public async Task<ActionResult<Result<List<ConversationMessageDto>>>> GetMessages(
        string conversationId, 
        [FromQuery] int? limit = null, 
        [FromQuery] DateTime? since = null)
    {
        try
        {
            var query = new GetConversationMessagesQuery(conversationId, limit, since);
            var result = await _mediator.Send(query);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving messages for conversation {ConversationId}", conversationId);
            return StatusCode(500, "An error occurred while retrieving messages");
        }
    }

    /// <summary>
    /// Test endpoint to trigger escalation notification - FOR DEVELOPMENT ONLY
    /// </summary>
    [HttpPost("test-escalation")]
    public async Task<ActionResult> TestEscalation()
    {
        try
        {
            var command = new EscalateConversationCommand(
                ConversationId: $"test-{Guid.NewGuid()}",
                Reason: "Test escalation for notification debugging",
                ConversationTranscript: "This is a test escalation to verify notifications work",
                WhatsAppPhoneNumber: "+27123456789"
            );
            
            var result = await _mediator.Send(command);
            
            if (result.Succeeded)
            {
                return Ok(new { message = "Test escalation triggered successfully", conversationId = result.Data });
            }
            else
            {
                return BadRequest(new { error = result.ErrorMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering test escalation");
            return StatusCode(500, "An error occurred while triggering test escalation");
        }
    }

    /// <summary>
    /// Notify an agent of a user message during an active conversation handoff
    /// Used by the bot to route user messages to agents in real-time
    /// </summary>
    /// <param name="conversationId">Bot Framework conversation ID</param>
    /// <param name="request">Agent notification details</param>
    /// <returns>Success result</returns>
    [HttpPost("{conversationId}/notify-agent")]
    public async Task<ActionResult<Result>> NotifyAgentOfUserMessage(
        string conversationId, 
        [FromBody] AgentNotificationRequest request)
    {
        try
        {
            _logger.LogInformation("Notifying agent {AgentId} of user message in conversation {ConversationId}", 
                request.AgentId, conversationId);

            // First, store the user message in the conversation history
            var messageDto = new ConversationMessageCreateDto
            {
                BotFrameworkConversationId = conversationId,
                Role = "user",
                Content = request.UserMessage,
                UserId = request.UserId,
                UserName = request.UserName,
                ChannelId = request.ChannelId,
                Timestamp = request.Timestamp
            };

            var addMessageCommand = new AddConversationMessageCommand(messageDto);
            var messageResult = await _mediator.Send(addMessageCommand);

            if (!messageResult.Succeeded)
            {
                _logger.LogError("Failed to store user message: {Error}", messageResult.ErrorMessage);
                return BadRequest(Result.Failure("Failed to store user message"));
            }

            // Send real-time SignalR notification to agents
            try
            {
                await _hubContext.Clients.All.NewConversationMessage(
                    conversationId,
                    request.UserName ?? request.UserId ?? "Unknown User",
                    request.UserMessage,
                    false // isFromAgent = false since this is from a user
                );

                _logger.LogInformation("SignalR notification sent for conversation {ConversationId}", conversationId);
            }
            catch (Exception signalREx)
            {
                _logger.LogError(signalREx, "Failed to send SignalR notification for conversation {ConversationId}", conversationId);
                // Don't fail the entire operation if SignalR notification fails
            }

            _logger.LogInformation("User message from conversation {ConversationId} successfully routed to agent {AgentId}. Message ID: {MessageId}", 
                conversationId, request.AgentId, messageResult.Data);

            return Ok(Result.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying agent {AgentId} of user message in conversation {ConversationId}", 
                request.AgentId, conversationId);
            return StatusCode(500, Result.Failure("An error occurred while notifying the agent"));
        }
    }

    /// <summary>
    /// Test endpoint to save agent preferences - FOR DEVELOPMENT ONLY
    /// </summary>
    [HttpPost("test-preferences")]
    public async Task<ActionResult> TestSavePreferences()
    {
        try
        {
            var command = new CleanArchitecture.Blazor.Application.Features.Agents.Commands.UpdatePreferences.UpdateAgentPreferencesCommand
            {
                ApplicationUserId = "test-user-id",
                EnableBrowserNotifications = true,
                EnableAudioAlerts = false,
                EnableEmailNotifications = true,
                NotifyOnStandardPriority = true,
                NotifyOnHighPriority = true,
                NotifyOnCriticalPriority = false,
                NotifyDuringBreak = false,
                NotifyWhenOffline = false,
                AudioVolume = 75
            };
            
            var result = await _mediator.Send(command);
            
            if (result.Succeeded)
            {
                return Ok(new { message = "Test preferences saved successfully", data = result.Data });
            }
            else
            {
                return BadRequest(new { error = result.ErrorMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving test preferences");
            return StatusCode(500, $"An error occurred while saving test preferences: {ex.Message}");
        }
    }

    /// <summary>
    /// Test endpoint to trigger a notification (for debugging)
    /// </summary>
    [HttpPost("test-notification")]
    public async Task<ActionResult<Result>> TestNotification()
    {
        try
        {
            _logger.LogInformation("Test notification endpoint called");

            // Send a test SignalR notification
            await _hubContext.Clients.All.NewConversationMessage(
                "test-conversation-123",
                "Test User",
                "This is a test message to verify notifications work",
                false // isFromAgent = false
            );

            _logger.LogInformation("Test SignalR notification sent");
            return Ok(Result.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test notification");
            return StatusCode(500, Result.Failure("Error sending test notification"));
        }
    }

    /// <summary>
    /// Test endpoint to trigger conversation assignment (for debugging)
    /// </summary>
    [HttpPost("test-assign")]
    public async Task<ActionResult<Result>> TestAssign([FromBody] TestAssignRequest request)
    {
        try
        {
            // Get current user ID from claims
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value
                        ?? User.Identity?.Name
                        ?? "test-user-123"; // fallback for testing
            
            var agentId = request.AgentId ?? userId; // Use current user as agent if not specified
            var conversationId = request.ConversationId ?? "test-conversation-123";
            
            _logger.LogInformation("Test assign endpoint called for conversation {ConversationId} to agent {AgentId} (current user: {CurrentUserId})", 
                conversationId, agentId, userId);

            // Send a test SignalR conversation assignment
            await _hubContext.Clients.All.ConversationAssigned(
                conversationId,
                agentId,
                User.Identity?.Name ?? "Test Agent"
            );

            _logger.LogInformation("Test SignalR conversation assignment sent");
            return Ok(Result.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test assignment");
            return StatusCode(500, Result.Failure("Error sending test assignment"));
        }
    }
}

/// <summary>
/// Request model for test assignment
/// </summary>
public class TestAssignRequest
{
    public string? ConversationId { get; set; }
    public string? AgentId { get; set; }
}

/// <summary>
/// Request model for escalating conversations
/// </summary>
public class EscalateConversationRequest
{
    public string ConversationId { get; set; } = default!;
    public string Reason { get; set; } = default!;
    public string? ConversationTranscript { get; set; }
    public string? WhatsAppPhoneNumber { get; set; }
}

/// <summary>
/// Request model for assigning agents
/// </summary>
public class AssignAgentRequest
{
    public string AgentId { get; set; } = default!;
}

/// <summary>
/// Request model for completing conversations
/// </summary>
public class CompleteConversationRequest
{
    public string Summary { get; set; } = default!;
}

/// <summary>
/// Request model for updating conversation status
/// </summary>
public class UpdateStatusRequest
{
    public string Status { get; set; } = default!;
}

/// <summary>
/// Request model for agent notifications during conversation handoff
/// </summary>
public class AgentNotificationRequest
{
    public string AgentId { get; set; } = default!;
    public string UserMessage { get; set; } = default!;
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? ChannelId { get; set; }
    public NotificationUrgency Urgency { get; set; } = NotificationUrgency.Normal;
}

/// <summary>
/// Urgency levels for agent notifications
/// </summary>
public enum NotificationUrgency
{
    Low,
    Normal,
    High,
    Critical
}

/// <summary>
/// DTO for conversation statistics
/// </summary>
public class ConversationStatsDto
{
    public int TotalConversations { get; set; }
    public int ActiveConversations { get; set; }
    public int CompletedConversations { get; set; }
    public int EscalatedConversations { get; set; }
    public int ConversationsToday { get; set; }
    public int ConversationsThisWeek { get; set; }
    public int ConversationsThisMonth { get; set; }
    public double AverageConversationLength { get; set; }
    public int ConversationsWithAttachments { get; set; }
}

/// <summary>
/// DTO for conversation analysis results
/// </summary>
public class ConversationAnalysisDto
{
    public string ConversationId { get; set; } = default!;
    public string Sentiment { get; set; } = default!;
    public List<string> Topics { get; set; } = new();
    public string IntentAnalysis { get; set; } = default!;
    public double SatisfactionScore { get; set; }
}

/// <summary>
/// DTO for bulk conversation analysis results
/// </summary>
public class ConversationsBulkAnalysisDto
{
    public int TotalAnalyzed { get; set; }
    public List<string> CommonTopics { get; set; } = new();
    public string OverallSentiment { get; set; } = default!;
    public double AverageSatisfactionScore { get; set; }
    public DateTime AnalysisTimestamp { get; set; }
}

/// <summary>
/// DTO for user intent insights
/// </summary>
public class UserIntentInsightsDto
{
    public List<string> CommonIntents { get; set; } = new();
    public Dictionary<string, int> IntentDistribution { get; set; } = new();
    public List<string> TrendingTopics { get; set; } = new();
    public DateTime AnalysisTimestamp { get; set; }
}
