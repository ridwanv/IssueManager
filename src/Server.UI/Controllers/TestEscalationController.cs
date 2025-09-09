using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using CleanArchitecture.Blazor.Server.UI.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Blazor.Server.UI.Controllers;

[ApiController]
[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
public class TestEscalationController : ControllerBase
{
    private readonly IApplicationHubWrapper _hubWrapper;
    private readonly IApplicationDbContextFactory _dbContextFactory;

    public TestEscalationController(
        IApplicationHubWrapper hubWrapper,
        IApplicationDbContextFactory dbContextFactory)
    {
        _hubWrapper = hubWrapper;
        _dbContextFactory = dbContextFactory;
    }

    [HttpPost("test-escalation")]
    public async Task<IActionResult> TestEscalation()
    {
        try
        {
            var conversationId = Guid.NewGuid().ToString("N")[..8];
            var reason = "Customer is frustrated and requesting immediate assistance";
            var customerPhone = "+1234567890";
            var customerName = "Test Customer";
            var priority = 2; // High priority
            var escalatedAt = DateTime.UtcNow;

            // Create persistent escalation in database
            await using var db = await _dbContextFactory.CreateAsync();
            var conversation = new Conversation
            {
                ConversationReference = conversationId,
                UserName = customerName,
                WhatsAppPhoneNumber = customerPhone,
                Status = ConversationStatus.Active,
                Mode = ConversationMode.Escalating,
                Priority = priority,
                EscalatedAt = escalatedAt,
                EscalationReason = reason,
                StartTime = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                TenantId = "default" // Use appropriate tenant ID
            };

            db.Conversations.Add(conversation);
            await db.SaveChangesAsync(CancellationToken.None);

            // Create the escalation popup DTO
            var escalationPopupDto = new EscalationPopupDto
            {
                ConversationReference = conversationId,
                CustomerName = customerName,
                PhoneNumber = customerPhone,
                EscalationReason = reason,
                Priority = priority,
                EscalatedAt = escalatedAt,
                LastMessage = "I need help with my order urgently!",
                MessageCount = 5,
                ConversationDuration = TimeSpan.FromMinutes(15),
                ConversationSummary = "Customer escalated due to delivery delay concerns"
            };

            // Send escalation popup to all agents (this should trigger the popup)
            await _hubWrapper.BroadcastEscalationPopupToAvailableAgents(escalationPopupDto);

            // Also broadcast persistent escalation notification (for the indicator)
            await _hubWrapper.BroadcastConversationEscalated(
                conversationId, reason, customerPhone);

            return Ok(new { 
                message = "Test escalation created and popup sent to agents",
                conversationId,
                reason,
                priority,
                escalatedAt,
                databaseId = conversation.Id,
                popupSent = true
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    [HttpPost("test-escalation-accept/{conversationId}")]
    public async Task<IActionResult> TestEscalationAccept(string conversationId)
    {
        try
        {
            // Update database - mark conversation as accepted by removing escalating mode
            await using var db = await _dbContextFactory.CreateAsync();
            var conversation = await db.Conversations
                .FirstOrDefaultAsync(c => c.ConversationReference == conversationId);

            if (conversation != null)
            {
                conversation.Mode = ConversationMode.Human;
                conversation.CurrentAgentId = "test-agent"; // In real scenario, this would be the actual agent ID
                await db.SaveChangesAsync(CancellationToken.None);
            }

            // Broadcast escalation acceptance to clear notifications
            await _hubWrapper.NotifyEscalationAccepted(conversationId, "test-agent");

            return Ok(new { 
                message = "Test escalation acceptance sent and database updated",
                conversationId,
                databaseUpdated = conversation != null
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }
}
