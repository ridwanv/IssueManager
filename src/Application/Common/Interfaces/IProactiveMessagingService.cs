using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Application.Common.Interfaces;

/// <summary>
/// Service for sending proactive messages to users via WhatsApp
/// </summary>
public interface IProactiveMessagingService
{
    /// <summary>
    /// Sends a proactive message about issue creation
    /// </summary>
    Task SendIssueCreatedMessageAsync(Issue issue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a proactive message about issue status change
    /// </summary>
    Task SendIssueStatusChangedMessageAsync(Issue issue, IssueStatus previousStatus, IssueStatus newStatus, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a proactive message about issue assignment
    /// </summary>
    Task SendIssueAssignedMessageAsync(Issue issue, string? previousAssignedUserId, string? newAssignedUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a proactive message about issue resolution
    /// </summary>
    Task SendIssueResolvedMessageAsync(Issue issue, string? resolutionNotes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a proactive message about a new comment on an issue
    /// </summary>
    Task SendIssueCommentAddedMessageAsync(Issue issue, string comment, string addedByUserId, CancellationToken cancellationToken = default);
}