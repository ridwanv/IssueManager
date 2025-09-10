// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Application.Common.Interfaces;

/// <summary>
/// Service for automatic assignment of escalated conversations to available agents
/// using configurable assignment strategies like round-robin
/// </summary>
public interface IAutoAssignmentService
{
    /// <summary>
    /// Attempts to automatically assign a conversation to an available agent
    /// based on the configured assignment strategy and agent availability
    /// </summary>
    /// <param name="conversationId">The conversation identifier (ConversationReference or Id)</param>
    /// <param name="tenantId">The tenant identifier for isolation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success/failure with assigned agent information</returns>
    Task<Result<AutoAssignmentResult>> AssignConversationAsync(string conversationId, string tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the auto-assignment configuration for a tenant
    /// </summary>
    /// <param name="tenantId">The tenant identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The auto-assignment configuration</returns>
    Task<AutoAssignmentSettings> GetAutoAssignmentSettingsAsync(string tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates the auto-assignment configuration for a tenant
    /// </summary>
    /// <param name="tenantId">The tenant identifier</param>
    /// <param name="settings">The new settings</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success/failure</returns>
    Task<Result<bool>> UpdateAutoAssignmentSettingsAsync(string tenantId, AutoAssignmentSettings settings, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if auto-assignment is enabled for a tenant
    /// </summary>
    /// <param name="tenantId">The tenant identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if auto-assignment is enabled</returns>
    Task<bool> IsAutoAssignmentEnabledAsync(string tenantId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of an auto-assignment operation
/// </summary>
public class AutoAssignmentResult
{
    public bool WasAssigned { get; set; }
    public string? AssignedAgentId { get; set; }
    public string? AssignedAgentName { get; set; }
    public string? Reason { get; set; }
    
    public static AutoAssignmentResult Success(string agentId, string agentName) =>
        new() { WasAssigned = true, AssignedAgentId = agentId, AssignedAgentName = agentName };
    
    public static AutoAssignmentResult Failed(string reason) =>
        new() { WasAssigned = false, Reason = reason };
}

/// <summary>
/// Auto-assignment configuration settings per tenant
/// </summary>
public class AutoAssignmentSettings
{
    public string TenantId { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public AssignmentStrategy Strategy { get; set; } = AssignmentStrategy.RoundRobin;
    public bool RespectAgentAvailability { get; set; } = true;
    public bool RespectWorkloadLimits { get; set; } = true;
    public int MaxRetryAttempts { get; set; } = 3;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public string? LastModifiedBy { get; set; }
}

/// <summary>
/// Available assignment strategies
/// </summary>
public enum AssignmentStrategy
{
    RoundRobin = 0,
    LeastLoaded = 1,
    Random = 2,
    SkillBased = 3
}