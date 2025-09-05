using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Blazor.Application.Features.Issues.EventHandlers;

public class IssueCommentAddedEventHandler : INotificationHandler<IssueCommentAddedEvent>
{
    private readonly ILogger<IssueCommentAddedEventHandler> _logger;
    private readonly IProactiveMessagingService _proactiveMessagingService;

    public IssueCommentAddedEventHandler(
        ILogger<IssueCommentAddedEventHandler> logger,
        IProactiveMessagingService proactiveMessagingService)
    {
        _logger = logger;
        _proactiveMessagingService = proactiveMessagingService;
    }

    public async Task Handle(IssueCommentAddedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling IssueCommentAddedEvent for issue {IssueId}", notification.Item.Id);

        try
        {
            await _proactiveMessagingService.SendIssueCommentAddedMessageAsync(
                notification.Item, 
                notification.Comment,
                notification.AddedByUserId,
                cancellationToken);
            
            _logger.LogInformation("Proactive message sent for issue comment added {IssueId}", notification.Item.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send proactive message for issue comment {IssueId}", notification.Item.Id);
        }
    }
}