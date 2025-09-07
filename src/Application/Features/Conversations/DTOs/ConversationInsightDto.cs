// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using CleanArchitecture.Blazor.Domain.Entities;

namespace CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;

/// <summary>
/// Data transfer object for conversation insights
/// </summary>
[Description("Conversation Insights")]
public class ConversationInsightDto
{
    [Description("Id")]
    public int Id { get; set; }

    [Description("Conversation Id")]
    public int ConversationId { get; set; }

    [Description("Sentiment Score")]
    public decimal SentimentScore { get; set; }

    [Description("Sentiment Label")]
    public string SentimentLabel { get; set; } = default!;

    [Description("Key Themes")]
    public List<string> KeyThemes { get; set; } = new();

    [Description("Resolution Success")]
    public bool? ResolutionSuccess { get; set; }

    [Description("Customer Satisfaction Indicators")]
    public List<string> CustomerSatisfactionIndicators { get; set; } = new();

    [Description("Recommendations")]
    public List<string> Recommendations { get; set; } = new();

    [Description("Processing Model")]
    public string ProcessingModel { get; set; } = default!;

    [Description("Processed At")]
    public DateTime ProcessedAt { get; set; }

    [Description("Processing Duration")]
    public TimeSpan ProcessingDuration { get; set; }

    [Description("Warnings")]
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Gets a formatted processing duration string
    /// </summary>
    public string ProcessingDurationFormatted => 
        ProcessingDuration.TotalSeconds < 1 
            ? $"{ProcessingDuration.Milliseconds}ms"
            : $"{ProcessingDuration.TotalSeconds:F1}s";

    /// <summary>
    /// Gets a summary of the key themes as a comma-separated string
    /// </summary>
    public string KeyThemesSummary => 
        KeyThemes.Any() ? string.Join(", ", KeyThemes.Take(3)) : "No themes identified";

    /// <summary>
    /// Gets the sentiment color for UI display
    /// </summary>
    public string SentimentColor => SentimentLabel.ToLower() switch
    {
        "positive" => "success",
        "negative" => "error", 
        "neutral" => "info",
        _ => "default"
    };

    /// <summary>
    /// Determines if the insight indicates a successful resolution
    /// </summary>
    public bool IsResolutionSuccessful => ResolutionSuccess == true;

    /// <summary>
    /// Gets the count of recommendations
    /// </summary>
    public int RecommendationCount => Recommendations.Count;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ConversationInsight, ConversationInsightDto>()
                .ForMember(dest => dest.KeyThemes, opt => opt.MapFrom<KeyThemesResolver>())
                .ForMember(dest => dest.CustomerSatisfactionIndicators, opt => opt.MapFrom<CustomerSatisfactionResolver>())
                .ForMember(dest => dest.Recommendations, opt => opt.MapFrom<RecommendationsResolver>())
                .ForMember(dest => dest.Warnings, opt => opt.MapFrom(src => new List<string>())); // Empty list for now
        }
    }

    private class KeyThemesResolver : IValueResolver<ConversationInsight, ConversationInsightDto, List<string>>
    {
        public List<string> Resolve(ConversationInsight source, ConversationInsightDto destination, List<string> destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source.KeyThemes))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(source.KeyThemes) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }

    private class CustomerSatisfactionResolver : IValueResolver<ConversationInsight, ConversationInsightDto, List<string>>
    {
        public List<string> Resolve(ConversationInsight source, ConversationInsightDto destination, List<string> destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source.CustomerSatisfactionIndicators))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(source.CustomerSatisfactionIndicators) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }

    private class RecommendationsResolver : IValueResolver<ConversationInsight, ConversationInsightDto, List<string>>
    {
        public List<string> Resolve(ConversationInsight source, ConversationInsightDto destination, List<string> destMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source.Recommendations))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(source.Recommendations) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}