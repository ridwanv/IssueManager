using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Caching;
using CleanArchitecture.Blazor.Application.Features.Issues.Caching;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CleanArchitecture.Blazor.Application.Features.Issues.Queries.GetDashboardMetrics;

public record GetDashboardMetricsQuery : ICacheableRequest<Result<DashboardMetricsDto>>
{
    public string CacheKey => IssueCacheKey.GetCacheKey($"dashboard-metrics");
    public MemoryCacheEntryOptions? Options => new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) // Cache for 5 minutes
    };
    public IEnumerable<string>? Tags => null; // Implemented for interface
}

public class GetDashboardMetricsQueryHandler : IRequestHandler<GetDashboardMetricsQuery, Result<DashboardMetricsDto>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;

    public GetDashboardMetricsQueryHandler(IApplicationDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Result<DashboardMetricsDto>> Handle(GetDashboardMetricsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
            
            var now = DateTime.UtcNow;
            var last24Hours = now.AddHours(-24);
            var last7Days = now.AddDays(-7);
            var last30Days = now.AddDays(-30);

            // Get all issues for calculations
            var allIssues = await db.Issues
                .Where(i => i.Created >= last30Days)
                .ToListAsync(cancellationToken);

            var openIssues = allIssues.Where(i => i.Status != IssueStatus.Closed && i.Status != IssueStatus.Resolved).ToList();
            var resolvedIssues = allIssues.Where(i => i.Status == IssueStatus.Resolved || i.Status == IssueStatus.Closed).ToList();
            
            // Calculate metrics
            var totalOpenIssues = openIssues.Count;
            var criticalIssues = openIssues.Count(i => i.Priority == IssuePriority.Critical);
            var newIssuesLast24h = allIssues.Count(i => i.Created >= last24Hours);
            var resolvedLast24h = resolvedIssues.Count(i => i.LastModified >= last24Hours);

            // Calculate average resolution time
            var avgResolutionTimeHours = resolvedIssues.Any() 
                ? resolvedIssues.Where(i => i.LastModified.HasValue)
                    .Average(i => (i.LastModified!.Value - i.Created!.Value).TotalHours)
                : 0;

            // Calculate SLA compliance (assuming 24h SLA for critical, 72h for others)
            var slaCompliantIssues = resolvedIssues.Count(i => 
            {
                if (!i.LastModified.HasValue || !i.Created.HasValue) return false;
                var resolutionTime = (i.LastModified.Value - i.Created.Value).TotalHours;
                return i.Priority == IssuePriority.Critical ? resolutionTime <= 24 : resolutionTime <= 72;
            });
            
            var slaCompliancePercentage = resolvedIssues.Any() 
                ? (double)slaCompliantIssues / resolvedIssues.Count * 100 
                : 100;

            // Status distribution
            var statusDistribution = allIssues
                .GroupBy(i => i.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            // Priority distribution
            var priorityDistribution = openIssues
                .GroupBy(i => i.Priority)
                .ToDictionary(g => g.Key, g => g.Count());

            // Category distribution
            var categoryDistribution = allIssues
                .Where(i => i.Created >= last7Days)
                .GroupBy(i => i.Category)
                .ToDictionary(g => g.Key, g => g.Count());

            // Channel distribution
            var channelDistribution = allIssues
                .Where(i => i.Created >= last7Days && !string.IsNullOrEmpty(i.Channel))
                .GroupBy(i => i.Channel)
                .ToDictionary(g => g.Key!, g => g.Count());

            // Trends (compare with previous period)
            var previousPeriodIssues = await db.Issues
                .Where(i => i.Created >= last30Days.AddDays(-30) && i.Created < last30Days)
                .CountAsync(cancellationToken);

            var currentPeriodIssues = allIssues.Count;
            var trendPercentage = previousPeriodIssues > 0 
                ? ((double)(currentPeriodIssues - previousPeriodIssues) / previousPeriodIssues) * 100 
                : 0;

            var metrics = new DashboardMetricsDto
            {
                // Core KPIs
                TotalOpenIssues = totalOpenIssues,
                CriticalIssues = criticalIssues,
                NewIssuesLast24Hours = newIssuesLast24h,
                ResolvedLast24Hours = resolvedLast24h,
                AverageResolutionTimeHours = avgResolutionTimeHours,
                SlaCompliancePercentage = slaCompliancePercentage,
                
                // Trend indicators
                TrendPercentage = trendPercentage,
                TrendDirection = trendPercentage > 0 ? "up" : trendPercentage < 0 ? "down" : "stable",
                
                // Distributions
                StatusDistribution = statusDistribution,
                PriorityDistribution = priorityDistribution,
                CategoryDistribution = categoryDistribution,
                ChannelDistribution = channelDistribution,
                
                // Meta data
                LastUpdated = now,
                DataPeriodDays = 30
            };

            return await Result<DashboardMetricsDto>.SuccessAsync(metrics);
        }
        catch (Exception ex)
        {
            return await Result<DashboardMetricsDto>.FailureAsync(new[] { ex.Message });
        }
    }
}

public class DashboardMetricsDto
{
    // Core KPIs
    public int TotalOpenIssues { get; set; }
    public int CriticalIssues { get; set; }
    public int NewIssuesLast24Hours { get; set; }
    public int ResolvedLast24Hours { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public double SlaCompliancePercentage { get; set; }
    
    // Trend Analysis
    public double TrendPercentage { get; set; }
    public string TrendDirection { get; set; } = "stable";
    
    // Distributions
    public Dictionary<IssueStatus, int> StatusDistribution { get; set; } = new();
    public Dictionary<IssuePriority, int> PriorityDistribution { get; set; } = new();
    public Dictionary<IssueCategory, int> CategoryDistribution { get; set; } = new();
    public Dictionary<string, int> ChannelDistribution { get; set; } = new();
    
    // Meta
    public DateTime LastUpdated { get; set; }
    public int DataPeriodDays { get; set; }
    
    // Computed Properties
    public string AverageResolutionTimeFormatted => 
        AverageResolutionTimeHours >= 24 
            ? $"{AverageResolutionTimeHours / 24:F1}d" 
            : $"{AverageResolutionTimeHours:F1}h";
    
    public string SlaComplianceFormatted => $"{SlaCompliancePercentage:F1}%";
    
    public string TrendIndicator => TrendDirection switch
    {
        "up" => $"↗ +{TrendPercentage:F1}%",
        "down" => $"↘ {TrendPercentage:F1}%",
        _ => "→ 0.0%"
    };
}