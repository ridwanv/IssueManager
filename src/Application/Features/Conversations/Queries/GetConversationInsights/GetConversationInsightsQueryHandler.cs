// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetConversationInsights;

/// <summary>
/// Handler for getting conversation insights by ID
/// </summary>
public class GetConversationInsightsByIdQueryHandler : IRequestHandler<GetConversationInsightsByIdQuery, Result<ConversationInsightDto?>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;
    private readonly ILogger<GetConversationInsightsByIdQueryHandler> _logger;

    public GetConversationInsightsByIdQueryHandler(
        IApplicationDbContextFactory dbContextFactory,
        IMapper mapper,
        ILogger<GetConversationInsightsByIdQueryHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ConversationInsightDto?>> Handle(GetConversationInsightsByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

            var insight = await db.ConversationInsights
                .FirstOrDefaultAsync(ci => ci.ConversationId == request.ConversationId, cancellationToken);

            if (insight == null)
            {
                _logger.LogDebug("No insights found for conversation {ConversationId}", request.ConversationId);
                return await Result<ConversationInsightDto?>.SuccessAsync(null);
            }

            var insightDto = _mapper.Map<ConversationInsightDto>(insight);

            _logger.LogDebug("Retrieved insights for conversation {ConversationId} with sentiment {SentimentLabel}",
                request.ConversationId, insight.SentimentLabel);

            return await Result<ConversationInsightDto?>.SuccessAsync(insightDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving insights for conversation {ConversationId}", request.ConversationId);
            return await Result<ConversationInsightDto?>.FailureAsync($"Error retrieving conversation insights: {ex.Message}");
        }
    }
}

/// <summary>
/// Handler for getting all conversation insights with filtering
/// </summary>
public class GetAllConversationInsightsQueryHandler : IRequestHandler<GetAllConversationInsightsQuery, Result<List<ConversationInsightDto>>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllConversationInsightsQueryHandler> _logger;

    public GetAllConversationInsightsQueryHandler(
        IApplicationDbContextFactory dbContextFactory,
        IMapper mapper,
        ILogger<GetAllConversationInsightsQueryHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<ConversationInsightDto>>> Handle(GetAllConversationInsightsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

            var query = db.ConversationInsights.AsQueryable();

            // Apply filters
            if (request.FromDate.HasValue)
            {
                query = query.Where(ci => ci.ProcessedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(ci => ci.ProcessedAt <= request.ToDate.Value);
            }

            if (!string.IsNullOrEmpty(request.SentimentFilter))
            {
                query = query.Where(ci => ci.SentimentLabel.ToLower() == request.SentimentFilter.ToLower());
            }

            if (request.ResolutionSuccessFilter.HasValue)
            {
                query = query.Where(ci => ci.ResolutionSuccess == request.ResolutionSuccessFilter.Value);
            }

            // Apply pagination
            var insights = await query
                .OrderByDescending(ci => ci.ProcessedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var insightDtos = _mapper.Map<List<ConversationInsightDto>>(insights);

            _logger.LogInformation("Retrieved {Count} conversation insights (page {PageNumber}, size {PageSize})",
                insightDtos.Count, request.PageNumber, request.PageSize);

            return await Result<List<ConversationInsightDto>>.SuccessAsync(insightDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation insights");
            return await Result<List<ConversationInsightDto>>.FailureAsync($"Error retrieving conversation insights: {ex.Message}");
        }
    }
}

/// <summary>
/// Handler for getting conversation insights summary/analytics
/// </summary>
public class GetConversationInsightsSummaryQueryHandler : IRequestHandler<GetConversationInsightsSummaryQuery, Result<ConversationInsightsSummaryDto>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly ILogger<GetConversationInsightsSummaryQueryHandler> _logger;

    public GetConversationInsightsSummaryQueryHandler(
        IApplicationDbContextFactory dbContextFactory,
        ILogger<GetConversationInsightsSummaryQueryHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task<Result<ConversationInsightsSummaryDto>> Handle(GetConversationInsightsSummaryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

            var query = db.ConversationInsights.AsQueryable();

            // Apply date filters
            if (request.FromDate.HasValue)
            {
                query = query.Where(ci => ci.ProcessedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(ci => ci.ProcessedAt <= request.ToDate.Value);
            }

            var insights = await query.ToListAsync(cancellationToken);

            if (!insights.Any())
            {
                return await Result<ConversationInsightsSummaryDto>.SuccessAsync(new ConversationInsightsSummaryDto());
            }

            // Calculate analytics
            var summary = new ConversationInsightsSummaryDto
            {
                TotalInsights = insights.Count,
                AverageSentimentScore = insights.Average(i => i.SentimentScore),
                SentimentDistribution = insights
                    .GroupBy(i => i.SentimentLabel)
                    .ToDictionary(g => g.Key, g => g.Count()),
                SuccessfulResolutions = insights.Count(i => i.ResolutionSuccess == true),
                UnsuccessfulResolutions = insights.Count(i => i.ResolutionSuccess == false),
                AverageProcessingDuration = TimeSpan.FromMilliseconds(insights.Average(i => i.ProcessingDuration.TotalMilliseconds)),
                ProcessingModelDistribution = insights
                    .GroupBy(i => i.ProcessingModel)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            // Calculate resolution success rate
            var totalWithResolutionStatus = summary.SuccessfulResolutions + summary.UnsuccessfulResolutions;
            summary.ResolutionSuccessRate = totalWithResolutionStatus > 0 
                ? (decimal)summary.SuccessfulResolutions / totalWithResolutionStatus 
                : 0;

            // Extract top themes and recommendations
            var allThemes = insights
                .SelectMany(i => string.IsNullOrEmpty(i.KeyThemes) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(i.KeyThemes) ?? new List<string>())
                .Where(theme => !string.IsNullOrWhiteSpace(theme))
                .GroupBy(theme => theme.ToLower())
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key)
                .ToList();

            var allRecommendations = insights
                .SelectMany(i => string.IsNullOrEmpty(i.Recommendations) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(i.Recommendations) ?? new List<string>())
                .Where(rec => !string.IsNullOrWhiteSpace(rec))
                .GroupBy(rec => rec.ToLower())
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key)
                .ToList();

            summary.TopKeyThemes = allThemes;
            summary.TopRecommendations = allRecommendations;

            _logger.LogInformation("Generated insights summary with {TotalInsights} insights, {AvgSentiment:F2} avg sentiment",
                summary.TotalInsights, summary.AverageSentimentScore);

            return await Result<ConversationInsightsSummaryDto>.SuccessAsync(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating conversation insights summary");
            return await Result<ConversationInsightsSummaryDto>.FailureAsync($"Error generating insights summary: {ex.Message}");
        }
    }
}