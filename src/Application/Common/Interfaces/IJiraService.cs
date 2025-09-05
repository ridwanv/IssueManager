// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Entities;

namespace CleanArchitecture.Blazor.Application.Common.Interfaces;

/// <summary>
/// Service interface for JIRA integration operations - Application layer interface
/// </summary>
public interface IJiraService
{
    /// <summary>
    /// Creates a new issue in JIRA based on the local Issue entity
    /// </summary>
    /// <param name="issue">The local issue to create in JIRA</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The JIRA issue key (e.g., "SUP-123")</returns>
    Task<string> CreateIssueAsync(Issue issue, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Tests the JIRA connection and authentication
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if connection is successful</returns>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}
