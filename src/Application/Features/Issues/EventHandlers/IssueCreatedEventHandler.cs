using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Blazor.Application.Features.Issues.EventHandlers;

public class IssueCreatedEventHandler : INotificationHandler<IssueCreatedEvent>
{
    private readonly ILogger<IssueCreatedEventHandler> _logger;
    private readonly IProactiveMessagingService _proactiveMessagingService;

    public IssueCreatedEventHandler(
        ILogger<IssueCreatedEventHandler> logger,
        IProactiveMessagingService proactiveMessagingService)
    {
        _logger = logger;
        _proactiveMessagingService = proactiveMessagingService;
    }

    public async Task Handle(IssueCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling IssueCreatedEvent for issue {IssueId}", notification.Item.Id);

        try
        {
            await _proactiveMessagingService.SendIssueCreatedMessageAsync(notification.Item, cancellationToken);
            _logger.LogInformation("Proactive message sent for issue creation {IssueId}", notification.Item.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send proactive message for issue creation {IssueId}", notification.Item.Id);
        }
    }
}