// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using Azure.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Infrastructure.Services;

public class IssueSimilarityService : IIssueSimilarityService
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<IssueSimilarityService> _logger;
    private readonly Kernel _kernel;
    private readonly string _deploymentName;

    public IssueSimilarityService(
        IApplicationDbContextFactory dbContextFactory,
        IMemoryCache cache,
        ILogger<IssueSimilarityService> logger,
        IConfiguration configuration)
    {
        _dbContextFactory = dbContextFactory;
        _cache = cache;
        _logger = logger;
        
        var endpoint = configuration["AZURE_OPENAI_API_ENDPOINT"] ?? throw new InvalidOperationException("AZURE_OPENAI_API_ENDPOINT configuration is required");
        var apiKey = configuration["AZURE_OPENAI_API_KEY"] ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY configuration is required");
        _deploymentName = configuration["AZURE_OPENAI_DEPLOYMENT_NAME"] ?? "gpt-4";
        
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddAzureOpenAIChatCompletion(_deploymentName, endpoint, apiKey);
        _kernel = kernelBuilder.Build();
    }

    public async Task<IEnumerable<SimilarIssueResult>> FindSimilarIssuesAsync(
        string title,
        string description,
        IssueCategory category,
        IssuePriority priority,
        string? product,
        string tenantId,
        int timeframeHours = 168,
        decimal confidenceThreshold = 0.8m,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"similarity_{title.GetHashCode()}_{description.GetHashCode()}_{tenantId}_{timeframeHours}";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<SimilarIssueResult>? cachedResult))
        {
            _logger.LogDebug("Returning cached similarity results for {CacheKey}", cacheKey);
            return cachedResult!;
        }

        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
            
            var cutoffTime = DateTime.UtcNow.AddHours(-timeframeHours);
            var recentIssues = await db.Issues
                .Where(i => i.TenantId == tenantId)
                .Where(i => i.Created >= cutoffTime)
                .Where(i => i.Status != IssueStatus.Closed)
                .OrderByDescending(i => i.Created)
                .Take(50) // Limit for performance and cost
                .ToListAsync(cancellationToken);

            if (!recentIssues.Any())
            {
                return Enumerable.Empty<SimilarIssueResult>();
            }

            var similarIssues = new List<SimilarIssueResult>();

            foreach (var issue in recentIssues)
            {
                var comparison = await CompareIssuesWithOpenAI(
                    title, description, category, priority, product,
                    issue.Title, issue.Description, issue.Category, issue.Priority, issue.Product,
                    cancellationToken);

                if (comparison.ConfidenceScore >= confidenceThreshold)
                {
                    similarIssues.Add(new SimilarIssueResult
                    {
                        Issue = issue,
                        ConfidenceScore = comparison.ConfidenceScore,
                        Reasoning = comparison.Analysis
                    });
                }
            }

            // Sort by confidence score descending
            var results = similarIssues.OrderByDescending(s => s.ConfidenceScore).ToList();
            
            // Cache results for 15 minutes
            _cache.Set(cacheKey, results, TimeSpan.FromMinutes(15));
            
            _logger.LogInformation("Found {Count} similar issues with confidence >= {Threshold}", 
                results.Count, confidenceThreshold);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar issues for title: {Title}", title);
            return Enumerable.Empty<SimilarIssueResult>();
        }
    }

    public async Task<SimilarityComparison> CompareIssuesAsync(
        Issue issue1,
        Issue issue2,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await CompareIssuesWithOpenAI(
                issue1.Title, issue1.Description, issue1.Category, issue1.Priority, issue1.Product,
                issue2.Title, issue2.Description, issue2.Category, issue2.Priority, issue2.Product,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing issues {Issue1Id} and {Issue2Id}", issue1.Id, issue2.Id);
            return new SimilarityComparison
            {
                ConfidenceScore = 0,
                IsSimilar = false,
                Analysis = "Error occurred during comparison",
                Details = new Dictionary<string, string> { { "Error", ex.Message } }
            };
        }
    }

    public async Task<IssueLinkRecommendation> AnalyzeRelationshipAsync(
        Issue existingIssue,
        Issue newIssue,
        decimal confidenceScore,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var prompt = CreateRelationshipAnalysisPrompt(existingIssue, newIssue, confidenceScore);
            var response = await CallOpenAIAsync(prompt, cancellationToken);
            
            return ParseRelationshipRecommendation(response, confidenceScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing relationship between issues {ExistingId} and {NewId}", 
                existingIssue.Id, newIssue.Id);
            
            return new IssueLinkRecommendation
            {
                RecommendedLinkType = confidenceScore >= 0.9m ? IssueLinkType.Duplicate : IssueLinkType.Related,
                Confidence = confidenceScore,
                Reasoning = "Automatic fallback recommendation due to analysis error",
                ShouldAutoLink = false,
                SuggestedResponse = "I found a similar issue but couldn't analyze the relationship. Please review manually."
            };
        }
    }

    private async Task<SimilarityComparison> CompareIssuesWithOpenAI(
        string title1, string desc1, IssueCategory cat1, IssuePriority pri1, string? prod1,
        string title2, string desc2, IssueCategory cat2, IssuePriority pri2, string? prod2,
        CancellationToken cancellationToken)
    {
        var prompt = CreateSimilarityPrompt(title1, desc1, cat1, pri1, prod1, title2, desc2, cat2, pri2, prod2);
        var response = await CallOpenAIAsync(prompt, cancellationToken);
        
        return ParseSimilarityResponse(response);
    }

    private string CreateSimilarityPrompt(
        string title1, string desc1, IssueCategory cat1, IssuePriority pri1, string? prod1,
        string title2, string desc2, IssueCategory cat2, IssuePriority pri2, string? prod2)
    {
        return $@"
You are an expert support analyst comparing two technical issues for similarity. 
Support both English and Afrikaans content naturally.

ISSUE A:
Title: {title1}
Description: {desc1}
Category: {cat1}
Priority: {pri1}
Product/System: {prod1 ?? "Not specified"}

ISSUE B:  
Title: {title2}
Description: {desc2}
Category: {cat2}
Priority: {pri2}
Product/System: {prod2 ?? "Not specified"}

Analyze these issues for semantic similarity considering:
1. Core problem similarity (login issues, payment errors, system crashes, etc.)
2. Affected system/product context
3. User impact and severity
4. Technical symptoms and error patterns
5. Multilingual context (English/Afrikaans mixed content)

Respond with JSON only:
{{
  ""confidence_score"": 0.0-1.0,
  ""is_similar"": boolean,
  ""analysis"": ""detailed explanation"",
  ""title_similarity"": 0.0-1.0,
  ""description_similarity"": 0.0-1.0,
  ""context_similarity"": 0.0-1.0,
  ""key_factors"": [""factor1"", ""factor2""]
}}

Confidence scoring guide:
- 0.9-1.0: Near identical issues (duplicates)
- 0.8-0.89: Very similar core problems  
- 0.7-0.79: Related issues, similar symptoms
- 0.6-0.69: Some similarity, possibly related
- 0.0-0.59: Different issues";
    }

    private string CreateRelationshipAnalysisPrompt(Issue existingIssue, Issue newIssue, decimal confidenceScore)
    {
        return $@"
You are an expert support manager analyzing the relationship between two similar issues.

EXISTING ISSUE (Status: {existingIssue.Status}):
Title: {existingIssue.Title}
Description: {existingIssue.Description}
Created: {existingIssue.Created:yyyy-MM-dd HH:mm}
Priority: {existingIssue.Priority}
Category: {existingIssue.Category}

NEW ISSUE:
Title: {newIssue.Title}  
Description: {newIssue.Description}
Priority: {newIssue.Priority}
Category: {newIssue.Category}

Similarity Confidence: {confidenceScore:P}

Determine the relationship and provide user response. Respond with JSON only:
{{
  ""relationship_type"": ""Duplicate|Related|CausedBy|PartOf|Blocks"",
  ""confidence"": 0.0-1.0,
  ""reasoning"": ""explanation"",
  ""should_auto_link"": boolean,
  ""suggested_response"": ""user-friendly message explaining we're aware of similar issue""
}}

Relationship types:
- Duplicate: Same core issue (confidence >= 0.9)
- Related: Similar symptoms but different root cause
- CausedBy: New issue is consequence of existing issue
- PartOf: Both part of larger incident/problem  
- Blocks: Existing issue prevents resolution of new issue

Auto-link only if confidence >= 0.85 for duplicates or >= 0.9 for other types.
Suggested response should be empathetic and informative.";
    }

    private async Task<string> CallOpenAIAsync(string prompt, CancellationToken cancellationToken)
    {
        try
        {
            var chatService = _kernel.GetRequiredService<IChatCompletionService>();
            
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage("You are a technical support analysis expert. Always respond with valid JSON only.");
            chatHistory.AddUserMessage(prompt);
            
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0.1, // Low temperature for consistent analysis
                MaxTokens = 1000
            };
            
            var response = await chatService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings,
                _kernel,
                cancellationToken);

            return response.Content ?? "{}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API");
            return "{}";
        }
    }

    private SimilarityComparison ParseSimilarityResponse(string response)
    {
        try
        {
            var json = JsonDocument.Parse(response);
            var root = json.RootElement;

            var confidence = root.TryGetProperty("confidence_score", out var confProp) ? 
                Convert.ToDecimal(confProp.GetDouble()) : 0m;

            return new SimilarityComparison
            {
                ConfidenceScore = confidence,
                IsSimilar = root.TryGetProperty("is_similar", out var simProp) && simProp.GetBoolean(),
                Analysis = root.TryGetProperty("analysis", out var analysisProp) ? 
                    analysisProp.GetString() ?? "" : "",
                Details = new Dictionary<string, string>
                {
                    ["title_similarity"] = root.TryGetProperty("title_similarity", out var titleProp) ? 
                        titleProp.GetDouble().ToString("P") : "N/A",
                    ["description_similarity"] = root.TryGetProperty("description_similarity", out var descProp) ? 
                        descProp.GetDouble().ToString("P") : "N/A",
                    ["context_similarity"] = root.TryGetProperty("context_similarity", out var contextProp) ? 
                        contextProp.GetDouble().ToString("P") : "N/A"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse OpenAI similarity response: {Response}", response);
            return new SimilarityComparison
            {
                ConfidenceScore = 0,
                IsSimilar = false,
                Analysis = "Failed to parse analysis response"
            };
        }
    }

    private IssueLinkRecommendation ParseRelationshipRecommendation(string response, decimal originalConfidence)
    {
        try
        {
            var json = JsonDocument.Parse(response);
            var root = json.RootElement;

            var relationshipStr = root.TryGetProperty("relationship_type", out var relProp) ? 
                relProp.GetString() : "Related";
            
            Enum.TryParse<IssueLinkType>(relationshipStr, out var linkType);

            return new IssueLinkRecommendation
            {
                RecommendedLinkType = linkType,
                Confidence = root.TryGetProperty("confidence", out var confProp) ? 
                    Convert.ToDecimal(confProp.GetDouble()) : originalConfidence,
                Reasoning = root.TryGetProperty("reasoning", out var reasonProp) ? 
                    reasonProp.GetString() ?? "" : "",
                ShouldAutoLink = root.TryGetProperty("should_auto_link", out var autoLinkProp) && 
                    autoLinkProp.GetBoolean(),
                SuggestedResponse = root.TryGetProperty("suggested_response", out var responseProp) ? 
                    responseProp.GetString() ?? "" : ""
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse relationship recommendation: {Response}", response);
            return new IssueLinkRecommendation
            {
                RecommendedLinkType = IssueLinkType.Related,
                Confidence = originalConfidence,
                Reasoning = "Failed to parse recommendation",
                ShouldAutoLink = false,
                SuggestedResponse = "We found a similar issue and are investigating the relationship."
            };
        }
    }
}