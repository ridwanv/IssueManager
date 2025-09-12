// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Application.Common.Constants.Roles;

/// <summary>
/// Global role names for ASP.NET Core Identity
/// All roles are system-wide (TenantId = null) with tenant-isolated assignments
/// </summary>
public abstract class RoleName
{
    // Legacy roles (to be deprecated)
    public const string Admin = nameof(Admin);
    public const string Basic = nameof(Basic);
    public const string Users = nameof(Users);

    // New persona-based roles matching UserType enum
    /// <summary>
    /// Platform super admin with cross-tenant access
    /// </summary>
    public const string PlatformOwner = nameof(PlatformOwner);
    
    /// <summary>
    /// Tenant administrator with full tenant management capabilities
    /// </summary>
    public const string TenantOwner = nameof(TenantOwner);
    
    /// <summary>
    /// Manages issues and assigns work within tenant
    /// </summary>
    public const string IssueManager = nameof(IssueManager);
    
    /// <summary>
    /// Works on assigned issues and updates status
    /// </summary>
    public const string IssueAssignee = nameof(IssueAssignee);
    
    /// <summary>
    /// Handles escalated customer conversations
    /// </summary>
    public const string ChatAgent = nameof(ChatAgent);
    
    /// <summary>
    /// Supervises chat agents and handles escalations
    /// </summary>
    public const string ChatSupervisor = nameof(ChatSupervisor);
    
    /// <summary>
    /// Basic user with limited system access
    /// </summary>
    public const string EndUser = nameof(EndUser);
    
    /// <summary>
    /// External system integration access
    /// </summary>
    public const string ApiConsumer = nameof(ApiConsumer);

    /// <summary>
    /// All available role names for system use
    /// </summary>
    public static readonly string[] AllRoles = new[]
    {
        // Legacy roles
        Admin, Basic, Users,
        // Persona-based roles
        PlatformOwner, TenantOwner, IssueManager, IssueAssignee,
        ChatAgent, ChatSupervisor, EndUser, ApiConsumer
    };

    /// <summary>
    /// New persona-based roles only (excluding legacy)
    /// </summary>
    public static readonly string[] PersonaRoles = new[]
    {
        PlatformOwner, TenantOwner, IssueManager, IssueAssignee,
        ChatAgent, ChatSupervisor, EndUser, ApiConsumer
    };
} 