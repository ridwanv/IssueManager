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

namespace CleanArchitecture.Blazor.Application.Features.Issues.Commands.LinkIssues;

[Authorize(Policy = Permissions.Issues.Edit)]
public class LinkIssuesCommandHandler : IRequestHandler<LinkIssuesCommand, Result<Guid>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly ILogger<LinkIssuesCommandHandler> _logger;
    private readonly IUserContextAccessor _currentUserService;

    public LinkIssuesCommandHandler(
        IApplicationDbContextFactory dbContextFactory,
        ILogger<LinkIssuesCommandHandler> logger,
        IUserContextAccessor currentUserService)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(LinkIssuesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

            // Verify both issues exist and belong to the same tenant
            var parentIssue = await db.Issues
                .FirstOrDefaultAsync(i => i.Id == request.ParentIssueId, cancellationToken);
                
            if (parentIssue == null)
            {
                return Result<Guid>.Failure("Parent issue not found");
            }

            var childIssue = await db.Issues
                .FirstOrDefaultAsync(i => i.Id == request.ChildIssueId, cancellationToken);
                
            if (childIssue == null)
            {
                return Result<Guid>.Failure("Child issue not found");
            }

            if (parentIssue.TenantId != childIssue.TenantId)
            {
                return Result<Guid>.Failure("Issues must belong to the same tenant");
            }

            // Check if link already exists
            var existingLink = await db.IssueLinks
                .FirstOrDefaultAsync(l => 
                    (l.ParentIssueId == request.ParentIssueId && l.ChildIssueId == request.ChildIssueId) ||
                    (l.ParentIssueId == request.ChildIssueId && l.ChildIssueId == request.ParentIssueId), 
                    cancellationToken);

            if (existingLink != null)
            {
                return Result<Guid>.Failure("A link between these issues already exists");
            }

            // Check for circular linking (prevent infinite loops)
            if (await HasCircularReference(db, request.ParentIssueId, request.ChildIssueId, cancellationToken))
            {
                return Result<Guid>.Failure("This link would create a circular reference");
            }

            // Create the issue link
            var issueLink = request.CreatedBySystem 
                ? IssueLink.CreateSystemLink(
                    request.ParentIssueId,
                    request.ChildIssueId,
                    request.LinkType,
                    request.ConfidenceScore ?? 0,
                    parentIssue.TenantId,
                    request.Metadata)
                : IssueLink.CreateManualLink(
                    request.ParentIssueId,
                    request.ChildIssueId,
                    request.LinkType,
                    parentIssue.TenantId,
                    request.Metadata);

            db.IssueLinks.Add(issueLink);

            // Add audit event logs to both issues
            var linkMetadata = new
            {
                LinkType = request.LinkType.ToString(),
                ConfidenceScore = request.ConfidenceScore,
                CreatedBySystem = request.CreatedBySystem,
                Reason = request.Reason,
                UserId = _currentUserService.Current?.UserId
            };

            var parentEventLog = new EventLog
            {
                Id = Guid.NewGuid(),
                IssueId = request.ParentIssueId,
                Type = "IssueLinked",
                Payload = JsonSerializer.Serialize(new { 
                    Action = "Linked",
                    Description = $"Linked to issue {childIssue.ReferenceNumber} as {request.LinkType}",
                    Metadata = linkMetadata
                }),
                CreatedUtc = DateTime.UtcNow,
                TenantId = parentIssue.TenantId
            };

            var childEventLog = new EventLog
            {
                Id = Guid.NewGuid(),
                IssueId = request.ChildIssueId,
                Type = "IssueLinked",
                Payload = JsonSerializer.Serialize(new { 
                    Action = "Linked",
                    Description = $"Linked to issue {parentIssue.ReferenceNumber} as child of {request.LinkType}",
                    Metadata = linkMetadata
                }),
                CreatedUtc = DateTime.UtcNow,
                TenantId = childIssue.TenantId
            };

            db.EventLogs.Add(parentEventLog);
            db.EventLogs.Add(childEventLog);

            // Add domain events for notifications
            issueLink.AddDomainEvent(new IssueLinkedEvent(issueLink, parentIssue, childIssue));

            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully linked issue {ParentId} to {ChildId} with type {LinkType}",
                request.ParentIssueId, request.ChildIssueId, request.LinkType);

            return Result<Guid>.Success(issueLink.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking issues {ParentId} and {ChildId}", 
                request.ParentIssueId, request.ChildIssueId);
            return Result<Guid>.Failure($"Failed to link issues: {ex.Message}");
        }
    }

    private async Task<bool> HasCircularReference(
        IApplicationDbContext db, 
        Guid proposedParentId, 
        Guid proposedChildId, 
        CancellationToken cancellationToken)
    {
        // Check if the proposed child is already a parent of the proposed parent
        // This prevents simple circular references (A -> B -> A)
        
        var existingParentLinks = await db.IssueLinks
            .Where(l => l.ChildIssueId == proposedParentId)
            .Select(l => l.ParentIssueId)
            .ToListAsync(cancellationToken);

        return existingParentLinks.Contains(proposedChildId);
        
        // Note: For more complex circular reference detection (A -> B -> C -> A), 
        // you would need a more sophisticated graph traversal algorithm
        // This simple check covers the most common case
    }
}

public class IssueLinkedEvent : DomainEvent
{
    public IssueLinkedEvent(IssueLink issueLink, Issue parentIssue, Issue childIssue)
    {
        IssueLink = issueLink;
        ParentIssue = parentIssue;
        ChildIssue = childIssue;
    }

    public IssueLink IssueLink { get; }
    public Issue ParentIssue { get; }
    public Issue ChildIssue { get; }
}