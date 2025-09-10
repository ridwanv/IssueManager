// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Interfaces.MultiTenant;
using CleanArchitecture.Blazor.Application.Features.Conversations.Commands.AssignAgent;
using CleanArchitecture.Blazor.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Blazor.Infrastructure.Services;

/// <summary>
/// Service for automatic assignment of escalated conversations to available agents
/// Implements round-robin and other assignment strategies with tenant isolation
/// </summary>
public class AutoAssignmentService : IAutoAssignmentService
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMediator _mediator;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AutoAssignmentService> _logger;
    private readonly ITenantService _tenantService;
    
    private const string CACHE_KEY_PREFIX = "auto_assignment_settings_";
    private const string LAST_ASSIGNED_AGENT_KEY = "last_assigned_agent_";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    public AutoAssignmentService(
        IApplicationDbContextFactory dbContextFactory,
        IMediator mediator,
        IMemoryCache cache,
        ILogger<AutoAssignmentService> logger,
        ITenantService tenantService)
    {
        _dbContextFactory = dbContextFactory;
        _mediator = mediator;
        _cache = cache;
        _logger = logger;
        _tenantService = tenantService;
    }

    public async Task<Result<AutoAssignmentResult>> AssignConversationAsync(string conversationId, string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting auto-assignment for conversation {ConversationId} in tenant {TenantId}", conversationId, tenantId);

            // Check if auto-assignment is enabled for this tenant
            var settings = await GetAutoAssignmentSettingsAsync(tenantId, cancellationToken);
            if (!settings.IsEnabled)
            {
                _logger.LogDebug("Auto-assignment is disabled for tenant {TenantId}", tenantId);
                return Result<AutoAssignmentResult>.Success(AutoAssignmentResult.Failed("Auto-assignment is disabled"));
            }

            // Get available agents for assignment
            var availableAgent = await GetNextAvailableAgentAsync(tenantId, settings.Strategy, cancellationToken);
            if (availableAgent == null)
            {
                _logger.LogWarning("No available agents found for auto-assignment in tenant {TenantId}", tenantId);
                return Result<AutoAssignmentResult>.Success(AutoAssignmentResult.Failed("No available agents"));
            }

            // Attempt assignment using existing AssignAgentCommand
            var assignResult = await _mediator.Send(new AssignAgentCommand(conversationId, availableAgent.ApplicationUserId), cancellationToken);
            
            if (assignResult.Succeeded)
            {
                // Update last assigned agent for round-robin
                if (settings.Strategy == AssignmentStrategy.RoundRobin)
                {
                    _cache.Set($"{LAST_ASSIGNED_AGENT_KEY}{tenantId}", availableAgent.ApplicationUserId, CacheExpiration);
                }

                var agentName = availableAgent.ApplicationUser?.DisplayName ?? 
                               availableAgent.ApplicationUser?.UserName ?? 
                               "Agent";

                _logger.LogInformation("Successfully auto-assigned conversation {ConversationId} to agent {AgentId} ({AgentName})", 
                    conversationId, availableAgent.ApplicationUserId, agentName);

                return Result<AutoAssignmentResult>.Success(AutoAssignmentResult.Success(availableAgent.ApplicationUserId, agentName));
            }
            else
            {
                _logger.LogWarning("Failed to auto-assign conversation {ConversationId}: {Error}", conversationId, assignResult.ErrorMessage);
                return Result<AutoAssignmentResult>.Success(AutoAssignmentResult.Failed(assignResult.ErrorMessage));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during auto-assignment for conversation {ConversationId}", conversationId);
            return Result<AutoAssignmentResult>.Failure($"Auto-assignment failed: {ex.Message}");
        }
    }

    public async Task<AutoAssignmentSettings> GetAutoAssignmentSettingsAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CACHE_KEY_PREFIX}{tenantId}";
        
        if (_cache.TryGetValue(cacheKey, out AutoAssignmentSettings? cachedSettings) && cachedSettings != null)
        {
            return cachedSettings;
        }

        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
        
        // For now, return default settings. In a full implementation, you'd store these in the database
        // This could be extended to use a dedicated AutoAssignmentSettings entity
        var settings = new AutoAssignmentSettings
        {
            TenantId = tenantId,
            IsEnabled = true, // Default to enabled
            Strategy = AssignmentStrategy.RoundRobin,
            RespectAgentAvailability = true,
            RespectWorkloadLimits = true,
            MaxRetryAttempts = 3
        };

        _cache.Set(cacheKey, settings, CacheExpiration);
        return settings;
    }

    public async Task<Result<bool>> UpdateAutoAssignmentSettingsAsync(string tenantId, AutoAssignmentSettings settings, CancellationToken cancellationToken = default)
    {
        try
        {
            // Update cache immediately
            var cacheKey = $"{CACHE_KEY_PREFIX}{tenantId}";
            settings.LastModified = DateTime.UtcNow;
            _cache.Set(cacheKey, settings, CacheExpiration);

            // In a full implementation, you would persist these settings to the database
            // For now, we're just using in-memory cache
            
            _logger.LogInformation("Updated auto-assignment settings for tenant {TenantId}: Enabled={IsEnabled}, Strategy={Strategy}", 
                tenantId, settings.IsEnabled, settings.Strategy);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating auto-assignment settings for tenant {TenantId}", tenantId);
            return Result<bool>.Failure($"Failed to update settings: {ex.Message}");
        }
    }

    public async Task<bool> IsAutoAssignmentEnabledAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var settings = await GetAutoAssignmentSettingsAsync(tenantId, cancellationToken);
        return settings.IsEnabled;
    }

    private async Task<Domain.Entities.Agent?> GetNextAvailableAgentAsync(string tenantId, AssignmentStrategy strategy, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

        // Get all available agents with capacity
        var availableAgents = await db.Agents
            .Include(a => a.ApplicationUser)
            .Where(a => a.TenantId == tenantId &&
                       a.Status == AgentStatus.Available &&
                       a.ActiveConversationCount < a.MaxConcurrentConversations)
            .ToListAsync(cancellationToken);

        if (!availableAgents.Any())
        {
            _logger.LogWarning("No available agents found in tenant {TenantId}", tenantId);
            return null;
        }

        return strategy switch
        {
            AssignmentStrategy.RoundRobin => GetRoundRobinAgent(availableAgents, tenantId),
            AssignmentStrategy.LeastLoaded => GetLeastLoadedAgent(availableAgents),
            AssignmentStrategy.Random => GetRandomAgent(availableAgents),
            _ => availableAgents.First() // Fallback
        };
    }

    private Domain.Entities.Agent GetRoundRobinAgent(List<Domain.Entities.Agent> availableAgents, string tenantId)
    {
        // Get the last assigned agent from cache
        var lastAssignedKey = $"{LAST_ASSIGNED_AGENT_KEY}{tenantId}";
        
        if (_cache.TryGetValue(lastAssignedKey, out string? lastAssignedAgentId) && !string.IsNullOrEmpty(lastAssignedAgentId))
        {
            var lastAgentIndex = availableAgents.FindIndex(a => a.ApplicationUserId == lastAssignedAgentId);
            if (lastAgentIndex >= 0)
            {
                // Get the next agent in the round-robin rotation
                var nextIndex = (lastAgentIndex + 1) % availableAgents.Count;
                return availableAgents[nextIndex];
            }
        }

        // If no previous agent or agent not found, return the first available agent
        return availableAgents.First();
    }

    private Domain.Entities.Agent GetLeastLoadedAgent(List<Domain.Entities.Agent> availableAgents)
    {
        return availableAgents
            .OrderBy(a => a.ActiveConversationCount)
            .ThenBy(a => (double)a.ActiveConversationCount / a.MaxConcurrentConversations) // Workload percentage
            .First();
    }

    private Domain.Entities.Agent GetRandomAgent(List<Domain.Entities.Agent> availableAgents)
    {
        var random = new Random();
        var index = random.Next(availableAgents.Count);
        return availableAgents[index];
    }
}