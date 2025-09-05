using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Caching;
using CleanArchitecture.Blazor.Application.Features.Issues.Caching;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CleanArchitecture.Blazor.Application.Features.Issues.Queries.GetRecentActivity;

public record GetRecentActivityQuery(int Count = 20) : ICacheableRequest<Result<List<RecentActivityDto>>>
{
    public string CacheKey => IssueCacheKey.GetCacheKey($"recent-activity-{Count}");
    public MemoryCacheEntryOptions? Options => new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2) // Cache for 2 minutes
    };
    public IEnumerable<string>? Tags => null; // Implemented for interface
}

public class GetRecentActivityQueryHandler : IRequestHandler<GetRecentActivityQuery, Result<List<RecentActivityDto>>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;

    public GetRecentActivityQueryHandler(IApplicationDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Result<List<RecentActivityDto>>> Handle(GetRecentActivityQuery request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
            
            var activities = new List<RecentActivityDto>();
            
            // Get recent issues
            var recentIssues = await db.Issues
                .OrderByDescending(i => i.Created)
                .Take(request.Count)
                .Select(i => new RecentActivityDto
                {
                    Id = i.Id,
                    Type = "issue_created",
                    Title = i.Title,
                    Description = $"New {i.Priority} priority {i.Category} issue created",
                    Timestamp = i.Created ?? DateTime.UtcNow,
                    Priority = i.Priority,
                    Status = i.Status,
                    Category = i.Category,
                    Channel = i.Channel,
                    ReferenceNumber = i.ReferenceNumber
                })
                .ToListAsync(cancellationToken);
            
            activities.AddRange(recentIssues);
            
            // Get recent event logs for status changes
            var recentEvents = await db.EventLogs
                .Where(e => e.Type == "status_changed" || e.Type == "priority_changed" || e.Type == "assigned")
                .OrderByDescending(e => e.CreatedUtc)
                .Take(request.Count)
                .Select(e => new
                {
                    e.Id,
                    e.IssueId,
                    e.Type,
                    e.Payload,
                    e.CreatedUtc,
                    Issue = e.Issue
                })
                .ToListAsync(cancellationToken);
            
            foreach (var eventLog in recentEvents)
            {
                if (eventLog.Issue == null) continue;
                
                var description = eventLog.Type switch
                {
                    "status_changed" => $"Status changed to {eventLog.Issue.Status}",
                    "priority_changed" => $"Priority changed to {eventLog.Issue.Priority}",
                    "assigned" => "Issue assigned to team member",
                    _ => "Issue updated"
                };
                
                activities.Add(new RecentActivityDto
                {
                    Id = eventLog.Issue.Id,
                    Type = eventLog.Type,
                    Title = eventLog.Issue.Title,
                    Description = description,
                    Timestamp = eventLog.CreatedUtc,
                    Priority = eventLog.Issue.Priority,
                    Status = eventLog.Issue.Status,
                    Category = eventLog.Issue.Category,
                    Channel = eventLog.Issue.Channel,
                    ReferenceNumber = eventLog.Issue.ReferenceNumber
                });
            }
            
            // Sort all activities by timestamp and take requested count
            var sortedActivities = activities
                .OrderByDescending(a => a.Timestamp)
                .Take(request.Count)
                .ToList();
            
            return await Result<List<RecentActivityDto>>.SuccessAsync(sortedActivities);
        }
        catch (Exception ex)
        {
            return await Result<List<RecentActivityDto>>.FailureAsync(new[] { ex.Message });
        }
    }
}

public class RecentActivityDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime Timestamp { get; set; }
    public IssuePriority Priority { get; set; }
    public IssueStatus Status { get; set; }
    public IssueCategory Category { get; set; }
    public string? Channel { get; set; }
    public string? ReferenceNumber { get; set; }
    // Helper properties
    public string TimeAgo
    {
        get
        {
            var timeSpan = DateTime.UtcNow - Timestamp;
            if (timeSpan.Days > 0)
                return $"{timeSpan.Days}d ago";
            if (timeSpan.Hours > 0)
                return $"{timeSpan.Hours}h ago";
            if (timeSpan.Minutes > 0)
                return $"{timeSpan.Minutes}m ago";
            return "Just now";
        }
    }
}