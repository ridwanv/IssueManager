using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Blazor.Application.Features.Issues.EventHandlers;

public class IssueResolvedEventHandler : INotificationHandler<IssueResolvedEvent>
{
    private readonly ILogger<IssueResolvedEventHandler> _logger;
    private readonly IProactiveMessagingService _proactiveMessagingService;

    public IssueResolvedEventHandler(
        ILogger<IssueResolvedEventHandler> logger,
        IProactiveMessagingService proactiveMessagingService)
    {
        _logger = logger;
        _proactiveMessagingService = proactiveMessagingService;
    }

    public async Task Handle(IssueResolvedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling IssueResolvedEvent for issue {IssueId}", notification.Item.Id);

        try
        {
            await _proactiveMessagingService.SendIssueResolvedMessageAsync(
                notification.Item, 
                notification.ResolutionNotes, 
                cancellationToken);
            
            _logger.LogInformation("Proactive message sent for issue resolution {IssueId}", notification.Item.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send proactive message for issue resolution {IssueId}", notification.Item.Id);
        }
    }
}