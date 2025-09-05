using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Caching;
using CleanArchitecture.Blazor.Application.Features.Issues.Caching;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CleanArchitecture.Blazor.Application.Features.Issues.Queries.GetPerformanceStats;

public record GetPerformanceStatsQuery(int Days = 30) : ICacheableRequest<Result<PerformanceStatsDto>>
{
    public string CacheKey => IssueCacheKey.GetCacheKey($"performance-stats-{Days}");
    public MemoryCacheEntryOptions? Options => new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes
    };
    public IEnumerable<string>? Tags => null; // Implemented for interface
}

public class GetPerformanceStatsQueryHandler : IRequestHandler<GetPerformanceStatsQuery, Result<PerformanceStatsDto>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;

    public GetPerformanceStatsQueryHandler(IApplicationDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Result<PerformanceStatsDto>> Handle(GetPerformanceStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
            
            var endDate = DateTime.UtcNow.Date.AddDays(1); // Tomorrow
            var startDate = endDate.AddDays(-request.Days);
            
            // Get issues for the specified period
            var issues = await db.Issues
                .Where(i => i.Created >= startDate && i.Created < endDate)
                .ToListAsync(cancellationToken);
            
            // Daily volume trends
            var dailyVolume = new Dictionary<DateTime, int>();
            for (var date = startDate.Date; date < endDate; date = date.AddDays(1))
            {
                var count = issues.Count(i => i.Created?.Date == date);
                dailyVolume[date] = count;
            }
            
            // Hourly distribution (24-hour pattern)
            var hourlyDistribution = new Dictionary<int, int>();
            for (int hour = 0; hour < 24; hour++)
            {
                hourlyDistribution[hour] = issues.Count(i => i.Created?.Hour == hour);
            }
            
            // Resolution time analysis
            var resolvedIssues = issues.Where(i => 
                (i.Status == IssueStatus.Resolved || i.Status == IssueStatus.Closed) && 
                i.LastModified.HasValue && i.Created.HasValue).ToList();
            
            var resolutionTimes = resolvedIssues
                .Select(i => (i.LastModified!.Value - i.Created!.Value).TotalHours)
                .OrderBy(h => h)
                .ToList();
            
            // Calculate percentiles
            double medianResolutionTime = 0;
            double p90ResolutionTime = 0;
            double p95ResolutionTime = 0;
            
            if (resolutionTimes.Any())
            {
                var sortedTimes = resolutionTimes.ToList();
                medianResolutionTime = GetPercentile(sortedTimes, 50);
                p90ResolutionTime = GetPercentile(sortedTimes, 90);
                p95ResolutionTime = GetPercentile(sortedTimes, 95);
            }
            
            // Category performance
            var categoryPerformance = issues
                .GroupBy(i => i.Category)
                .Select(g => new CategoryPerformanceDto
                {
                    Category = g.Key,
                    TotalIssues = g.Count(),
                    ResolvedIssues = g.Count(i => i.Status == IssueStatus.Resolved || i.Status == IssueStatus.Closed),
                    AverageResolutionHours = g.Where(i => 
                        (i.Status == IssueStatus.Resolved || i.Status == IssueStatus.Closed) && 
                        i.LastModified.HasValue && i.Created.HasValue)
                        .Select(i => (i.LastModified!.Value - i.Created!.Value).TotalHours)
                        .DefaultIfEmpty(0)
                        .Average()
                })
                .ToList();
            
            // Priority performance
            var priorityPerformance = issues
                .GroupBy(i => i.Priority)
                .Select(g => new PriorityPerformanceDto
                {
                    Priority = g.Key,
                    TotalIssues = g.Count(),
                    ResolvedIssues = g.Count(i => i.Status == IssueStatus.Resolved || i.Status == IssueStatus.Closed),
                    AverageResolutionHours = g.Where(i => 
                        (i.Status == IssueStatus.Resolved || i.Status == IssueStatus.Closed) && 
                        i.LastModified.HasValue && i.Created.HasValue)
                        .Select(i => (i.LastModified!.Value - i.Created!.Value).TotalHours)
                        .DefaultIfEmpty(0)
                        .Average()
                })
                .ToList();
            
            // Channel performance
            var channelPerformance = issues
                .Where(i => !string.IsNullOrEmpty(i.Channel))
                .GroupBy(i => i.Channel)
                .Select(g => new ChannelPerformanceDto
                {
                    Channel = g.Key!,
                    TotalIssues = g.Count(),
                    ResolvedIssues = g.Count(i => i.Status == IssueStatus.Resolved || i.Status == IssueStatus.Closed),
                    AverageResolutionHours = g.Where(i => 
                        (i.Status == IssueStatus.Resolved || i.Status == IssueStatus.Closed) && 
                        i.LastModified.HasValue && i.Created.HasValue)
                        .Select(i => (i.LastModified!.Value - i.Created!.Value).TotalHours)
                        .DefaultIfEmpty(0)
                        .Average()
                })
                .ToList();
            
            var stats = new PerformanceStatsDto
            {
                // Time series data
                DailyVolumeChart = dailyVolume.Select(kv => new ChartDataPoint
                {
                    Label = kv.Key.ToString("MMM dd"),
                    Value = kv.Value,
                    Date = kv.Key
                }).ToList(),
                
                HourlyDistribution = hourlyDistribution.Select(kv => new ChartDataPoint
                {
                    Label = $"{kv.Key:00}:00",
                    Value = kv.Value
                }).ToList(),
                
                // Resolution metrics
                MedianResolutionTimeHours = medianResolutionTime,
                P90ResolutionTimeHours = p90ResolutionTime,
                P95ResolutionTimeHours = p95ResolutionTime,
                TotalResolvedIssues = resolvedIssues.Count,
                
                // Performance breakdowns
                CategoryPerformance = categoryPerformance,
                PriorityPerformance = priorityPerformance,
                ChannelPerformance = channelPerformance,
                
                // Meta
                PeriodDays = request.Days,
                GeneratedAt = DateTime.UtcNow
            };
            
            return await Result<PerformanceStatsDto>.SuccessAsync(stats);
        }
        catch (Exception ex)
        {
            return await Result<PerformanceStatsDto>.FailureAsync(new[] { ex.Message });
        }
    }
    
    private static double GetPercentile(List<double> sortedValues, int percentile)
    {
        if (!sortedValues.Any()) return 0;
        
        var index = (double)(percentile * sortedValues.Count) / 100 - 1;
        
        if (index < 0) return sortedValues[0];
        if (index >= sortedValues.Count - 1) return sortedValues[^1];
        
        var lowerIndex = (int)Math.Floor(index);
        var upperIndex = lowerIndex + 1;
        var fraction = index - lowerIndex;
        
        return sortedValues[lowerIndex] + (sortedValues[upperIndex] - sortedValues[lowerIndex]) * fraction;
    }
}

public class PerformanceStatsDto
{
    // Chart data
    public List<ChartDataPoint> DailyVolumeChart { get; set; } = new();
    public List<ChartDataPoint> HourlyDistribution { get; set; } = new();
    
    // Resolution time metrics
    public double MedianResolutionTimeHours { get; set; }
    public double P90ResolutionTimeHours { get; set; }
    public double P95ResolutionTimeHours { get; set; }
    public int TotalResolvedIssues { get; set; }
    
    // Performance breakdowns
    public List<CategoryPerformanceDto> CategoryPerformance { get; set; } = new();
    public List<PriorityPerformanceDto> PriorityPerformance { get; set; } = new();
    public List<ChannelPerformanceDto> ChannelPerformance { get; set; } = new();
    
    // Meta
    public int PeriodDays { get; set; }
    public DateTime GeneratedAt { get; set; }
    
    // Computed properties
    public string MedianResolutionFormatted => FormatHours(MedianResolutionTimeHours);
    public string P90ResolutionFormatted => FormatHours(P90ResolutionTimeHours);
    public string P95ResolutionFormatted => FormatHours(P95ResolutionTimeHours);
    
    private static string FormatHours(double hours)
    {
        if (hours >= 24)
            return $"{hours / 24:F1}d";
        return $"{hours:F1}h";
    }
}

public class ChartDataPoint
{
    public string Label { get; set; } = default!;
    public double Value { get; set; }
    public DateTime? Date { get; set; }
}

public class CategoryPerformanceDto
{
    public IssueCategory Category { get; set; }
    public int TotalIssues { get; set; }
    public int ResolvedIssues { get; set; }
    public double AverageResolutionHours { get; set; }
    
    public double ResolutionRate => TotalIssues > 0 ? (double)ResolvedIssues / TotalIssues * 100 : 0;
    public string ResolutionRateFormatted => $"{ResolutionRate:F1}%";
}

public class PriorityPerformanceDto
{
    public IssuePriority Priority { get; set; }
    public int TotalIssues { get; set; }
    public int ResolvedIssues { get; set; }
    public double AverageResolutionHours { get; set; }
    
    public double ResolutionRate => TotalIssues > 0 ? (double)ResolvedIssues / TotalIssues * 100 : 0;
    public string ResolutionRateFormatted => $"{ResolutionRate:F1}%";
}

public class ChannelPerformanceDto
{
    public string Channel { get; set; } = default!;
    public int TotalIssues { get; set; }
    public int ResolvedIssues { get; set; }
    public double AverageResolutionHours { get; set; }
    
    public double ResolutionRate => TotalIssues > 0 ? (double)ResolvedIssues / TotalIssues * 100 : 0;
    public string ResolutionRateFormatted => $"{ResolutionRate:F1}%";
}