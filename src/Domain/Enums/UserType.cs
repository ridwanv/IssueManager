// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Domain.Enums;

/// <summary>
/// Defines the type/persona of a user in the system
/// </summary>
public enum UserType
{
    /// <summary>
    /// Platform super admin with cross-tenant access
    /// </summary>
    PlatformOwner = 1,
    
    /// <summary>
    /// Tenant administrator with full tenant management capabilities
    /// </summary>
    TenantOwner = 2,
    
    /// <summary>
    /// Manages issues and assigns work within tenant
    /// </summary>
    IssueManager = 3,
    
    /// <summary>
    /// Works on assigned issues and updates status
    /// </summary>
    IssueAssignee = 4,
    
    /// <summary>
    /// Handles escalated customer conversations
    /// </summary>
    ChatAgent = 5,
    
    /// <summary>
    /// Supervises chat agents and handles escalations
    /// </summary>
    ChatSupervisor = 6,
    
    /// <summary>
    /// Basic user with limited system access
    /// </summary>
    EndUser = 7,
    
    /// <summary>
    /// External system integration access
    /// </summary>
    ApiConsumer = 8
}