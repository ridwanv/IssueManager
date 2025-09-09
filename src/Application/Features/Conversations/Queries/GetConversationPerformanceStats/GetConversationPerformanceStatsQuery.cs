using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Caching;
using CleanArchitecture.Blazor.Application.Features.Conversations.Caching;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetConversationPerformanceStats;

public record GetConversationPerformanceStatsQuery(int Days = 30) : ICacheableRequest<Result<ConversationPerformanceStatsDto>>
{
    public string CacheKey => ConversationCacheKey.GetCacheKey($"performance-stats-{Days}");
    public MemoryCacheEntryOptions? Options => new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes
    };
    public IEnumerable<string>? Tags => null; // Implemented for interface
}

public class GetConversationPerformanceStatsQueryHandler : IRequestHandler<GetConversationPerformanceStatsQuery, Result<ConversationPerformanceStatsDto>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;

    public GetConversationPerformanceStatsQueryHandler(IApplicationDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Result<ConversationPerformanceStatsDto>> Handle(GetConversationPerformanceStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
            
            var endDate = DateTime.UtcNow.Date.AddDays(1); // Tomorrow
            var startDate = endDate.AddDays(-request.Days);
            
            // Get conversations for the specified period
            var conversations = await db.Conversations
                .Include(c => c.ConversationInsight)
                .Where(c => c.Created >= startDate && c.Created < endDate)
                .ToListAsync(cancellationToken);
            
            // Daily volume trends
            var dailyVolume = new Dictionary<DateTime, int>();
            for (var date = startDate.Date; date < endDate; date = date.AddDays(1))
            {
                var count = conversations.Count(c => c.Created?.Date == date);
                dailyVolume[date] = count;
            }
            
            // Hourly distribution (24-hour pattern)
            var hourlyDistribution = new Dictionary<int, int>();
            for (int hour = 0; hour < 24; hour++)
            {
                hourlyDistribution[hour] = conversations.Count(c => c.Created?.Hour == hour);
            }
            
            // Resolution time analysis
            var completedConversations = conversations.Where(c => 
                c.Status == ConversationStatus.Completed && 
                c.CompletedAt.HasValue && c.Created.HasValue).ToList();
            
            var resolutionTimes = completedConversations
                .Select(c => (c.CompletedAt!.Value - c.Created!.Value).TotalHours)
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
            
            // Sentiment trends over time
            var conversationsWithInsights = conversations.Where(c => c.ConversationInsight != null).ToList();
            var sentimentTrends = new Dictionary<DateTime, double>();
            for (var date = startDate.Date; date < endDate; date = date.AddDays(1))
            {
                var dayConversations = conversationsWithInsights.Where(c => c.Created?.Date == date).ToList();
                var avgSentiment = dayConversations.Any() 
                    ? (double)dayConversations.Average(c => c.ConversationInsight!.SentimentScore)
                    : 0;
                sentimentTrends[date] = avgSentiment;
            }

            // Agent performance 
            var agentPerformance = conversations
                .Where(c => !string.IsNullOrEmpty(c.CurrentAgentId))
                .GroupBy(c => c.CurrentAgentId)
                .Select(g => new AgentPerformanceDto
                {
                    AgentId = g.Key!,
                    AgentName = $"Agent {g.Key}", // TODO: Get agent name from Users table
                    TotalConversations = g.Count(),
                    CompletedConversations = g.Count(c => c.Status == ConversationStatus.Completed),
                    EscalatedConversations = g.Count(c => c.Mode == ConversationMode.Human || c.Mode == ConversationMode.Escalating),
                    AverageResolutionHours = g.Where(c => 
                        c.Status == ConversationStatus.Completed && 
                        c.CompletedAt.HasValue && c.Created.HasValue)
                        .Select(c => (c.CompletedAt!.Value - c.Created!.Value).TotalHours)
                        .DefaultIfEmpty(0)
                        .Average(),
                    AverageSentimentScore = g.Where(c => c.ConversationInsight != null)
                        .Select(c => (double)c.ConversationInsight!.SentimentScore)
                        .DefaultIfEmpty(0)
                        .Average()
                })
                .ToList();
            
            // Channel performance (based on conversation mode/type)
            var channelPerformance = conversations
                .GroupBy(c => c.Mode)
                .Select(g => new ConversationChannelPerformanceDto
                {
                    Channel = g.Key.ToString(),
                    TotalConversations = g.Count(),
                    CompletedConversations = g.Count(c => c.Status == ConversationStatus.Completed),
                    EscalatedConversations = g.Count(c => c.Mode == ConversationMode.Human || c.Mode == ConversationMode.Escalating),
                    AverageResolutionHours = g.Where(c => 
                        c.Status == ConversationStatus.Completed && 
                        c.CompletedAt.HasValue && c.Created.HasValue)
                        .Select(c => (c.CompletedAt!.Value - c.Created!.Value).TotalHours)
                        .DefaultIfEmpty(0)
                        .Average(),
                    AverageSentimentScore = g.Where(c => c.ConversationInsight != null)
                        .Select(c => (double)c.ConversationInsight!.SentimentScore)
                        .DefaultIfEmpty(0)
                        .Average()
                })
                .ToList();

            // Resolution category performance
            var resolutionPerformance = completedConversations
                .Where(c => c.ResolutionCategory.HasValue)
                .GroupBy(c => c.ResolutionCategory!.Value)
                .Select(g => new ResolutionCategoryPerformanceDto
                {
                    Category = g.Key,
                    TotalConversations = g.Count(),
                    AverageResolutionHours = g.Where(c => c.CompletedAt.HasValue && c.Created.HasValue)
                        .Select(c => (c.CompletedAt!.Value - c.Created!.Value).TotalHours)
                        .DefaultIfEmpty(0)
                        .Average(),
                    AverageSentimentScore = g.Where(c => c.ConversationInsight != null)
                        .Select(c => (double)c.ConversationInsight!.SentimentScore)
                        .DefaultIfEmpty(0)
                        .Average()
                })
                .ToList();
            
            var stats = new ConversationPerformanceStatsDto
            {
                // Time series data
                DailyVolumeChart = dailyVolume.Select(kv => new ConversationChartDataPoint
                {
                    Label = kv.Key.ToString("MMM dd"),
                    Value = kv.Value,
                    Date = kv.Key
                }).ToList(),
                
                HourlyDistribution = hourlyDistribution.Select(kv => new ConversationChartDataPoint
                {
                    Label = $"{kv.Key:00}:00",
                    Value = kv.Value
                }).ToList(),

                SentimentTrendsChart = sentimentTrends.Select(kv => new ConversationChartDataPoint
                {
                    Label = kv.Key.ToString("MMM dd"),
                    Value = kv.Value,
                    Date = kv.Key
                }).ToList(),
                
                // Resolution metrics
                MedianResolutionTimeHours = medianResolutionTime,
                P90ResolutionTimeHours = p90ResolutionTime,
                P95ResolutionTimeHours = p95ResolutionTime,
                TotalCompletedConversations = completedConversations.Count,
                
                // Performance breakdowns
                AgentPerformance = agentPerformance,
                ChannelPerformance = channelPerformance,
                ResolutionCategoryPerformance = resolutionPerformance,
                
                // Meta
                PeriodDays = request.Days,
                GeneratedAt = DateTime.UtcNow
            };
            
            return await Result<ConversationPerformanceStatsDto>.SuccessAsync(stats);
        }
        catch (Exception ex)
        {
            return await Result<ConversationPerformanceStatsDto>.FailureAsync(new[] { ex.Message });
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

public class ConversationPerformanceStatsDto
{
    // Chart data
    public List<ConversationChartDataPoint> DailyVolumeChart { get; set; } = new();
    public List<ConversationChartDataPoint> HourlyDistribution { get; set; } = new();
    public List<ConversationChartDataPoint> SentimentTrendsChart { get; set; } = new();
    
    // Resolution time metrics
    public double MedianResolutionTimeHours { get; set; }
    public double P90ResolutionTimeHours { get; set; }
    public double P95ResolutionTimeHours { get; set; }
    public int TotalCompletedConversations { get; set; }
    
    // Performance breakdowns
    public List<AgentPerformanceDto> AgentPerformance { get; set; } = new();
    public List<ConversationChannelPerformanceDto> ChannelPerformance { get; set; } = new();
    public List<ResolutionCategoryPerformanceDto> ResolutionCategoryPerformance { get; set; } = new();
    
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

public class ConversationChartDataPoint
{
    public string Label { get; set; } = default!;
    public double Value { get; set; }
    public DateTime? Date { get; set; }
}

public class AgentPerformanceDto
{
    public string AgentId { get; set; } = default!;
    public string AgentName { get; set; } = default!;
    public int TotalConversations { get; set; }
    public int CompletedConversations { get; set; }
    public int EscalatedConversations { get; set; }
    public double AverageResolutionHours { get; set; }
    public double AverageSentimentScore { get; set; }
    
    public double CompletionRate => TotalConversations > 0 ? (double)CompletedConversations / TotalConversations * 100 : 0;
    public double EscalationRate => TotalConversations > 0 ? (double)EscalatedConversations / TotalConversations * 100 : 0;
    public string CompletionRateFormatted => $"{CompletionRate:F1}%";
    public string EscalationRateFormatted => $"{EscalationRate:F1}%";
    public string SentimentScoreFormatted => $"{AverageSentimentScore:F2}";
}

public class ConversationChannelPerformanceDto
{
    public string Channel { get; set; } = default!;
    public int TotalConversations { get; set; }
    public int CompletedConversations { get; set; }
    public int EscalatedConversations { get; set; }
    public double AverageResolutionHours { get; set; }
    public double AverageSentimentScore { get; set; }
    
    public double CompletionRate => TotalConversations > 0 ? (double)CompletedConversations / TotalConversations * 100 : 0;
    public double EscalationRate => TotalConversations > 0 ? (double)EscalatedConversations / TotalConversations * 100 : 0;
    public string CompletionRateFormatted => $"{CompletionRate:F1}%";
    public string EscalationRateFormatted => $"{EscalationRate:F1}%";
    public string SentimentScoreFormatted => $"{AverageSentimentScore:F2}";
}

public class ResolutionCategoryPerformanceDto
{
    public ResolutionCategory Category { get; set; }
    public int TotalConversations { get; set; }
    public double AverageResolutionHours { get; set; }
    public double AverageSentimentScore { get; set; }
    
    public string CategoryName => Category.ToString();
    public string SentimentScoreFormatted => $"{AverageSentimentScore:F2}";
}
