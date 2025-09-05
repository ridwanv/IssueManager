using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Blazor.Application.Features.Issues.EventHandlers;

public class IssueStatusChangedEventHandler : INotificationHandler<IssueStatusChangedEvent>
{
    private readonly ILogger<IssueStatusChangedEventHandler> _logger;
    private readonly IProactiveMessagingService _proactiveMessagingService;

    public IssueStatusChangedEventHandler(
        ILogger<IssueStatusChangedEventHandler> logger,
        IProactiveMessagingService proactiveMessagingService)
    {
        _logger = logger;
        _proactiveMessagingService = proactiveMessagingService;
    }

    public async Task Handle(IssueStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling IssueStatusChangedEvent for issue {IssueId}", notification.Item.Id);

        try
        {
            await _proactiveMessagingService.SendIssueStatusChangedMessageAsync(
                notification.Item, 
                notification.PreviousStatus, 
                notification.NewStatus, 
                cancellationToken);
            
            _logger.LogInformation("Proactive message sent for issue status change {IssueId}: {PreviousStatus} → {NewStatus}", 
                notification.Item.Id, notification.PreviousStatus, notification.NewStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send proactive message for issue status change {IssueId}", notification.Item.Id);
        }
    }
}