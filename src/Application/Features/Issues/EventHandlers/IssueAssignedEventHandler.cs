using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Blazor.Application.Features.Issues.EventHandlers;

public class IssueAssignedEventHandler : INotificationHandler<IssueAssignedEvent>
{
    private readonly ILogger<IssueAssignedEventHandler> _logger;
    private readonly IProactiveMessagingService _proactiveMessagingService;

    public IssueAssignedEventHandler(
        ILogger<IssueAssignedEventHandler> logger,
        IProactiveMessagingService proactiveMessagingService)
    {
        _logger = logger;
        _proactiveMessagingService = proactiveMessagingService;
    }

    public async Task Handle(IssueAssignedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling IssueAssignedEvent for issue {IssueId}", notification.Item.Id);

        try
        {
            await _proactiveMessagingService.SendIssueAssignedMessageAsync(
                notification.Item, 
                notification.PreviousAssignedUserId, 
                notification.NewAssignedUserId, 
                cancellationToken);
            
            _logger.LogInformation("Proactive message sent for issue assignment {IssueId}: {PreviousUser} → {NewUser}", 
                notification.Item.Id, notification.PreviousAssignedUserId, notification.NewAssignedUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send proactive message for issue assignment {IssueId}", notification.Item.Id);
        }
    }
}