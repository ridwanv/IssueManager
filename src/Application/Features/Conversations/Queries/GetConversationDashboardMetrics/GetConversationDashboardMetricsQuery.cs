using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Caching;
using CleanArchitecture.Blazor.Application.Features.Conversations.Caching;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetConversationDashboardMetrics;

public record GetConversationDashboardMetricsQuery : ICacheableRequest<Result<ConversationDashboardMetricsDto>>
{
    public string CacheKey => ConversationCacheKey.GetCacheKey($"dashboard-metrics");
    public MemoryCacheEntryOptions? Options => new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) // Cache for 5 minutes
    };
    public IEnumerable<string>? Tags => null; // Implemented for interface
}

public class GetConversationDashboardMetricsQueryHandler : IRequestHandler<GetConversationDashboardMetricsQuery, Result<ConversationDashboardMetricsDto>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;

    public GetConversationDashboardMetricsQueryHandler(IApplicationDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Result<ConversationDashboardMetricsDto>> Handle(GetConversationDashboardMetricsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
            
            var now = DateTime.UtcNow;
            var last24Hours = now.AddHours(-24);
            var last7Days = now.AddDays(-7);
            var last30Days = now.AddDays(-30);

            // Get all conversations for calculations
            var allConversations = await db.Conversations
                .Include(c => c.ConversationInsight)
                .Where(c => c.Created >= last30Days)
                .ToListAsync(cancellationToken);

            var activeConversations = allConversations.Where(c => c.Status == ConversationStatus.Active).ToList();
            var completedConversations = allConversations.Where(c => c.Status == ConversationStatus.Completed).ToList();
            var escalatedConversations = allConversations.Where(c => c.Mode == ConversationMode.Human || c.Mode == ConversationMode.Escalating).ToList();
            
            // Calculate metrics
            var totalConversations = allConversations.Count;
            var escalationRate = totalConversations > 0 ? (double)escalatedConversations.Count / totalConversations * 100 : 0;
            var newConversationsLast24h = allConversations.Count(c => c.Created >= last24Hours);
            var completedLast24h = completedConversations.Count(c => c.CompletedAt >= last24Hours);

            // Calculate average resolution time (for completed conversations)
            var avgResolutionTimeHours = completedConversations.Any(c => c.CompletedAt.HasValue) 
                ? completedConversations.Where(c => c.CompletedAt.HasValue)
                    .Average(c => (c.CompletedAt!.Value - c.Created!.Value).TotalHours)
                : 0;

            // Calculate average sentiment score
            var conversationsWithInsights = allConversations.Where(c => c.ConversationInsight != null).ToList();
            var avgSentimentScore = conversationsWithInsights.Any() 
                ? (double)conversationsWithInsights.Average(c => c.ConversationInsight!.SentimentScore) 
                : 0;

            // Calculate agent response time (approximation based on message count and duration)
            var activeConversationsWithMessages = allConversations.Where(c => c.MessageCount > 1).ToList();
            var avgAgentResponseTimeMinutes = activeConversationsWithMessages.Any()
                ? activeConversationsWithMessages.Average(c => c.Duration.TotalMinutes / Math.Max(c.MessageCount - 1, 1))
                : 0;

            // Calculate customer satisfaction score (based on positive sentiment and resolution success)
            var satisfactionIndicators = conversationsWithInsights.Where(c => 
                c.ConversationInsight!.ResolutionSuccess == true || 
                c.ConversationInsight!.SentimentScore > 0.3m).ToList();
            
            var customerSatisfactionScore = conversationsWithInsights.Any() 
                ? (double)satisfactionIndicators.Count / conversationsWithInsights.Count * 100 
                : 0;

            // Status distribution
            var statusDistribution = allConversations
                .GroupBy(c => c.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            // Mode distribution
            var modeDistribution = allConversations
                .GroupBy(c => c.Mode)
                .ToDictionary(g => g.Key, g => g.Count());

            // Priority distribution
            var priorityDistribution = allConversations
                .GroupBy(c => c.Priority)
                .ToDictionary(g => g.Key, g => g.Count());

            // Sentiment distribution
            var sentimentDistribution = conversationsWithInsights
                .GroupBy(c => c.ConversationInsight!.SentimentLabel)
                .ToDictionary(g => g.Key, g => g.Count());

            // Resolution category distribution
            var resolutionDistribution = completedConversations
                .Where(c => c.ResolutionCategory.HasValue)
                .GroupBy(c => c.ResolutionCategory!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            // Trends (compare with previous period)
            var previousPeriodConversations = await db.Conversations
                .Where(c => c.Created >= last30Days.AddDays(-30) && c.Created < last30Days)
                .CountAsync(cancellationToken);

            var currentPeriodConversations = allConversations.Count;
            var trendPercentage = previousPeriodConversations > 0 
                ? ((double)(currentPeriodConversations - previousPeriodConversations) / previousPeriodConversations) * 100 
                : 0;

            var metrics = new ConversationDashboardMetricsDto
            {
                // Core KPIs
                TotalConversations = totalConversations,
                AverageResolutionTimeHours = avgResolutionTimeHours,
                AverageSentimentScore = avgSentimentScore,
                EscalationRate = escalationRate,
                AgentResponseTimeMinutes = avgAgentResponseTimeMinutes,
                CustomerSatisfactionScore = customerSatisfactionScore,
                
                // Activity metrics
                NewConversationsLast24Hours = newConversationsLast24h,
                CompletedLast24Hours = completedLast24h,
                ActiveConversations = activeConversations.Count,
                EscalatedConversations = escalatedConversations.Count,
                
                // Trend indicators
                TrendPercentage = trendPercentage,
                TrendDirection = trendPercentage > 0 ? "up" : trendPercentage < 0 ? "down" : "stable",
                
                // Distributions
                StatusDistribution = statusDistribution,
                ModeDistribution = modeDistribution,
                PriorityDistribution = priorityDistribution,
                SentimentDistribution = sentimentDistribution,
                ResolutionDistribution = resolutionDistribution,
                
                // Meta data
                LastUpdated = now,
                DataPeriodDays = 30
            };

            return await Result<ConversationDashboardMetricsDto>.SuccessAsync(metrics);
        }
        catch (Exception ex)
        {
            return await Result<ConversationDashboardMetricsDto>.FailureAsync(new[] { ex.Message });
        }
    }
}

public class ConversationDashboardMetricsDto
{
    // Core KPIs
    public int TotalConversations { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public double AverageSentimentScore { get; set; }
    public double EscalationRate { get; set; }
    public double AgentResponseTimeMinutes { get; set; }
    public double CustomerSatisfactionScore { get; set; }
    
    // Activity metrics
    public int NewConversationsLast24Hours { get; set; }
    public int CompletedLast24Hours { get; set; }
    public int ActiveConversations { get; set; }
    public int EscalatedConversations { get; set; }
    
    // Trend Analysis
    public double TrendPercentage { get; set; }
    public string TrendDirection { get; set; } = "stable";
    
    // Distributions
    public Dictionary<ConversationStatus, int> StatusDistribution { get; set; } = new();
    public Dictionary<ConversationMode, int> ModeDistribution { get; set; } = new();
    public Dictionary<int, int> PriorityDistribution { get; set; } = new();
    public Dictionary<string, int> SentimentDistribution { get; set; } = new();
    public Dictionary<ResolutionCategory, int> ResolutionDistribution { get; set; } = new();
    
    // Meta
    public DateTime LastUpdated { get; set; }
    public int DataPeriodDays { get; set; }
    
    // Computed Properties
    public string AverageResolutionTimeFormatted => 
        AverageResolutionTimeHours >= 24 
            ? $"{AverageResolutionTimeHours / 24:F1}d" 
            : $"{AverageResolutionTimeHours:F1}h";
    
    public string AgentResponseTimeFormatted =>
        AgentResponseTimeMinutes >= 60
            ? $"{AgentResponseTimeMinutes / 60:F1}h"
            : $"{AgentResponseTimeMinutes:F1}m";
    
    public string EscalationRateFormatted => $"{EscalationRate:F1}%";
    
    public string CustomerSatisfactionFormatted => $"{CustomerSatisfactionScore:F1}%";
    
    public string SentimentScoreFormatted => $"{AverageSentimentScore:F2}";
    
    public string SentimentLabel => AverageSentimentScore switch
    {
        >= 0.1 => "Positive",
        <= -0.1 => "Negative",
        _ => "Neutral"
    };
    
    public string TrendIndicator => TrendDirection switch
    {
        "up" => $"↗ +{TrendPercentage:F1}%",
        "down" => $"↘ {TrendPercentage:F1}%",
        _ => "→ 0.0%"
    };
}
