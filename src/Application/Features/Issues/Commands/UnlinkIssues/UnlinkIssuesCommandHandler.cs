// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Security;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;
using CleanArchitecture.Blazor.Domain.Common;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CleanArchitecture.Blazor.Application.Features.Issues.Commands.UnlinkIssues;

[Authorize(Policy = Permissions.Issues.Edit)]
public class UnlinkIssuesCommandHandler : IRequestHandler<UnlinkIssuesCommand, Result<bool>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly ILogger<UnlinkIssuesCommandHandler> _logger;
    private readonly IUserContextAccessor _currentUserService;

    public UnlinkIssuesCommandHandler(
        IApplicationDbContextFactory dbContextFactory,
        ILogger<UnlinkIssuesCommandHandler> logger,
        IUserContextAccessor currentUserService)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(UnlinkIssuesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

            // Find the issue link to remove
            var issueLink = await db.IssueLinks
                .Include(l => l.ParentIssue)
                .Include(l => l.ChildIssue)
                .FirstOrDefaultAsync(l => l.Id == request.IssueLinkId, cancellationToken);

            if (issueLink == null)
            {
                return Result<bool>.Failure("Issue link not found");
            }

            // Store information for logging before deletion
            var parentIssue = issueLink.ParentIssue;
            var childIssue = issueLink.ChildIssue;
            var linkType = issueLink.LinkType;

            // Remove the issue link
            db.IssueLinks.Remove(issueLink);

            // Add audit event logs to both issues
            var unlinkMetadata = new
            {
                LinkType = linkType.ToString(),
                RemovedLinkId = issueLink.Id,
                Reason = request.Reason,
                UserId = _currentUserService.Current?.UserId,
                UnlinkedAt = DateTime.UtcNow
            };

            var parentEventLog = new EventLog
            {
                Id = Guid.NewGuid(),
                IssueId = parentIssue.Id,
                Type = "IssueUnlinked",
                Payload = JsonSerializer.Serialize(new { 
                    Action = "Unlinked",
                    Description = $"Unlinked from issue {childIssue.ReferenceNumber} ({linkType})",
                    Metadata = unlinkMetadata
                }),
                CreatedUtc = DateTime.UtcNow,
                TenantId = parentIssue.TenantId
            };

            var childEventLog = new EventLog
            {
                Id = Guid.NewGuid(),
                IssueId = childIssue.Id,
                Type = "IssueUnlinked",
                Payload = JsonSerializer.Serialize(new { 
                    Action = "Unlinked",
                    Description = $"Unlinked from issue {parentIssue.ReferenceNumber} ({linkType})",
                    Metadata = unlinkMetadata
                }),
                CreatedUtc = DateTime.UtcNow,
                TenantId = childIssue.TenantId
            };

            db.EventLogs.Add(parentEventLog);
            db.EventLogs.Add(childEventLog);

            // Add domain event for notifications
            issueLink.AddDomainEvent(new IssueUnlinkedEvent(issueLink, parentIssue, childIssue, request.Reason));

            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully unlinked issues {ParentId} and {ChildId} (Link ID: {LinkId})",
                parentIssue.Id, childIssue.Id, request.IssueLinkId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking issues for link ID {LinkId}", request.IssueLinkId);
            return Result<bool>.Failure($"Failed to unlink issues: {ex.Message}");
        }
    }
}

public class IssueUnlinkedEvent : DomainEvent
{
    public IssueUnlinkedEvent(IssueLink issueLink, Issue parentIssue, Issue childIssue, string? reason)
    {
        IssueLink = issueLink;
        ParentIssue = parentIssue;
        ChildIssue = childIssue;
        Reason = reason;
    }

    public IssueLink IssueLink { get; }
    public Issue ParentIssue { get; }
    public Issue ChildIssue { get; }
    public string? Reason { get; }
}