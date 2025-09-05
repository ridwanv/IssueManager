// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using AutoMapper;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Security;
using CleanArchitecture.Blazor.Application.Features.Issues.DTOs;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Blazor.Application.Features.Issues.Queries.GetLinkedIssues;

[Authorize(Policy = Permissions.Issues.View)]
public class GetLinkedIssuesQueryHandler : IRequestHandler<GetLinkedIssuesQuery, Result<LinkedIssuesDto>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;
    private readonly ILogger<GetLinkedIssuesQueryHandler> _logger;

    public GetLinkedIssuesQueryHandler(
        IApplicationDbContextFactory dbContextFactory,
        IMapper mapper,
        ILogger<GetLinkedIssuesQueryHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<LinkedIssuesDto>> Handle(GetLinkedIssuesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

            // Get the main issue
            var issue = await db.Issues
                .FirstOrDefaultAsync(i => i.Id == request.IssueId, cancellationToken);

            if (issue == null)
            {
                return Result<LinkedIssuesDto>.Failure("Issue not found");
            }

            // Get all links where this issue is the parent
            var childLinks = await db.IssueLinks
                .Where(l => l.ParentIssueId == request.IssueId)
                .Include(l => l.ChildIssue)
                    .ThenInclude(i => i.ReporterContact)
                .ToListAsync(cancellationToken);

            // Get all links where this issue is the child
            var parentLinks = await db.IssueLinks
                .Where(l => l.ChildIssueId == request.IssueId)
                .Include(l => l.ParentIssue)
                    .ThenInclude(i => i.ReporterContact)
                .ToListAsync(cancellationToken);

            // Create the result DTO
            var result = new LinkedIssuesDto
            {
                IssueId = issue.Id,
                IssueReferenceNumber = issue.ReferenceNumber,
                IssueTitle = issue.Title,
                ParentLinks = parentLinks.Select(l => new IssueLinkDto
                {
                    Id = l.Id,
                    ParentIssueId = l.ParentIssueId,
                    ChildIssueId = l.ChildIssueId,
                    LinkType = l.LinkType,
                    ConfidenceScore = l.ConfidenceScore,
                    CreatedBySystem = l.CreatedBySystem,
                    Metadata = l.Metadata,
                    Created = l.Created,
                    CreatedBy = l.CreatedBy,
                    RelatedIssue = _mapper.Map<IssueDto>(l.ParentIssue),
                    RelationshipDescription = GetRelationshipDescription(l.LinkType, false)
                }).ToList(),
                ChildLinks = childLinks.Select(l => new IssueLinkDto
                {
                    Id = l.Id,
                    ParentIssueId = l.ParentIssueId,
                    ChildIssueId = l.ChildIssueId,
                    LinkType = l.LinkType,
                    ConfidenceScore = l.ConfidenceScore,
                    CreatedBySystem = l.CreatedBySystem,
                    Metadata = l.Metadata,
                    Created = l.Created,
                    CreatedBy = l.CreatedBy,
                    RelatedIssue = _mapper.Map<IssueDto>(l.ChildIssue),
                    RelationshipDescription = GetRelationshipDescription(l.LinkType, true)
                }).ToList()
            };

            // Calculate impact summary if requested
            if (request.IncludeDetails)
            {
                result.ImpactSummary = await CalculateImpactSummary(db, issue, childLinks, parentLinks, cancellationToken);
            }

            _logger.LogDebug("Retrieved {ParentLinks} parent links and {ChildLinks} child links for issue {IssueId}",
                parentLinks.Count, childLinks.Count, request.IssueId);

            return Result<LinkedIssuesDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving linked issues for issue {IssueId}", request.IssueId);
            return Result<LinkedIssuesDto>.Failure($"Failed to retrieve linked issues: {ex.Message}");
        }
    }

    private async Task<IssueImpactSummaryDto> CalculateImpactSummary(
        IApplicationDbContext db,
        Issue mainIssue,
        IList<IssueLink> childLinks,
        IList<IssueLink> parentLinks,
        CancellationToken cancellationToken)
    {
        // Get all related issue IDs
        var allRelatedIssueIds = new List<Guid> { mainIssue.Id };
        allRelatedIssueIds.AddRange(childLinks.Select(l => l.ChildIssueId));
        allRelatedIssueIds.AddRange(parentLinks.Select(l => l.ParentIssueId));

        // Get detailed information for all related issues
        var allRelatedIssues = await db.Issues
            .Where(i => allRelatedIssueIds.Contains(i.Id))
            .Include(i => i.ReporterContact)
            .ToListAsync(cancellationToken);

        var affectedUsers = new List<AffectedUserDto>();
        var priorityBreakdown = new Dictionary<IssuePriority, int>();
        var statusBreakdown = new Dictionary<IssueStatus, int>();

        foreach (var relatedIssue in allRelatedIssues)
        {
            // Add to affected users
            affectedUsers.Add(new AffectedUserDto
            {
                IssueId = relatedIssue.Id,
                IssueReference = relatedIssue.ReferenceNumber,
                ContactName = relatedIssue.ReporterContact?.Name ?? relatedIssue.ReporterName,
                ContactPhone = relatedIssue.ReporterContact?.PhoneNumber ?? relatedIssue.ReporterPhone,
                IssueDescription = relatedIssue.Description,
                ReportedAt = relatedIssue.Created ?? DateTime.UtcNow,
                Priority = relatedIssue.Priority,
                Status = relatedIssue.Status
            });

            // Update priority breakdown
            priorityBreakdown[relatedIssue.Priority] = priorityBreakdown.GetValueOrDefault(relatedIssue.Priority, 0) + 1;
            
            // Update status breakdown
            statusBreakdown[relatedIssue.Status] = statusBreakdown.GetValueOrDefault(relatedIssue.Status, 0) + 1;
        }

        return new IssueImpactSummaryDto
        {
            AffectedUserCount = affectedUsers.Count,
            AffectedUsers = affectedUsers.OrderBy(u => u.ReportedAt).ToList(),
            PriorityBreakdown = priorityBreakdown,
            StatusBreakdown = statusBreakdown,
            EarliestReported = affectedUsers.Min(u => u.ReportedAt),
            LatestReported = affectedUsers.Max(u => u.ReportedAt)
        };
    }

    private string GetRelationshipDescription(IssueLinkType linkType, bool isParentToChild)
    {
        return linkType switch
        {
            IssueLinkType.Duplicate when isParentToChild => "This is a duplicate of the related issue",
            IssueLinkType.Duplicate when !isParentToChild => "The related issue is a duplicate of this one",
            IssueLinkType.Related => "This issue is related to the linked issue",
            IssueLinkType.Blocks when isParentToChild => "This issue blocks the related issue",
            IssueLinkType.Blocks when !isParentToChild => "This issue is blocked by the related issue",
            IssueLinkType.CausedBy when isParentToChild => "This issue is caused by the related issue",
            IssueLinkType.CausedBy when !isParentToChild => "This issue causes the related issue",
            IssueLinkType.PartOf when isParentToChild => "This issue is part of the related issue",
            IssueLinkType.PartOf when !isParentToChild => "The related issue is part of this issue",
            _ => "This issue is linked to the related issue"
        };
    }
}