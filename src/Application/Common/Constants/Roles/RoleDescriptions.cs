// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace CleanArchitecture.Blazor.Application.Common.Constants.Roles;

/// <summary>
/// Detailed descriptions and permission mappings for all system roles
/// </summary>
public static class RoleDescriptions
{
    /// <summary>
    /// Role descriptions with their intended purpose and capabilities
    /// </summary>
    public static readonly Dictionary<string, RoleInfo> Roles = new()
    {
        // Legacy Roles (to be deprecated)
        [RoleName.Admin] = new RoleInfo
        {
            Name = RoleName.Admin,
            DisplayName = "Administrator (Legacy)",
            Description = "Legacy administrator role - to be replaced by TenantOwner",
            IsLegacy = true,
            TenantScoped = true,
            Capabilities = new[]
            {
                "Full tenant administration",
                "User management",
                "System configuration"
            }
        },

        [RoleName.Basic] = new RoleInfo
        {
            Name = RoleName.Basic,
            DisplayName = "Basic User (Legacy)",
            Description = "Legacy basic user role - to be replaced by EndUser",
            IsLegacy = true,
            TenantScoped = true,
            Capabilities = new[]
            {
                "Basic system access",
                "Limited functionality"
            }
        },

        [RoleName.Users] = new RoleInfo
        {
            Name = RoleName.Users,
            DisplayName = "Users (Legacy)",
            Description = "Legacy users role - to be replaced by persona-specific roles",
            IsLegacy = true,
            TenantScoped = true,
            Capabilities = new[]
            {
                "Standard user access",
                "General system usage"
            }
        },

        // Persona-Based Roles
        [RoleName.PlatformOwner] = new RoleInfo
        {
            Name = RoleName.PlatformOwner,
            DisplayName = "Platform Owner",
            Description = "Platform super admin with cross-tenant access and system management",
            IsLegacy = false,
            TenantScoped = false, // Can access multiple tenants
            Capabilities = new[]
            {
                "Cross-tenant system administration",
                "Platform configuration and management",
                "Global user and tenant management",
                "System monitoring and maintenance",
                "Security and compliance oversight",
                "API and integration management"
            }
        },

        [RoleName.TenantOwner] = new RoleInfo
        {
            Name = RoleName.TenantOwner,
            DisplayName = "Tenant Owner",
            Description = "Tenant administrator with full management capabilities within their tenant",
            IsLegacy = false,
            TenantScoped = true,
            Capabilities = new[]
            {
                "Complete tenant administration",
                "User management within tenant",
                "Role assignment and permissions",
                "Tenant configuration and settings",
                "Billing and subscription management",
                "Security and audit oversight",
                "Data export and backup management"
            }
        },

        [RoleName.IssueManager] = new RoleInfo
        {
            Name = RoleName.IssueManager,
            DisplayName = "Issue Manager",
            Description = "Manages issues and assigns work within tenant boundaries",
            IsLegacy = false,
            TenantScoped = true,
            Capabilities = new[]
            {
                "Issue creation and management",
                "Work assignment and delegation",
                "Priority and category management",
                "Progress tracking and reporting",
                "Team coordination",
                "Issue lifecycle management",
                "Performance analytics"
            }
        },

        [RoleName.IssueAssignee] = new RoleInfo
        {
            Name = RoleName.IssueAssignee,
            DisplayName = "Issue Assignee",
            Description = "Works on assigned issues and updates status within tenant",
            IsLegacy = false,
            TenantScoped = true,
            Capabilities = new[]
            {
                "View assigned issues",
                "Update issue status and progress",
                "Add comments and attachments",
                "Log time and activities",
                "Request assistance or escalation",
                "Access relevant documentation",
                "Collaborate with team members"
            }
        },

        [RoleName.ChatAgent] = new RoleInfo
        {
            Name = RoleName.ChatAgent,
            DisplayName = "Chat Agent",
            Description = "Handles escalated customer conversations and support interactions",
            IsLegacy = false,
            TenantScoped = true,
            Capabilities = new[]
            {
                "Handle escalated customer conversations",
                "Access customer support tools",
                "Create issues from customer interactions",
                "Update customer contact information",
                "Access knowledge base and documentation",
                "Escalate complex issues to supervisors",
                "Track conversation metrics"
            }
        },

        [RoleName.ChatSupervisor] = new RoleInfo
        {
            Name = RoleName.ChatSupervisor,
            DisplayName = "Chat Supervisor",
            Description = "Supervises chat agents and handles complex escalations",
            IsLegacy = false,
            TenantScoped = true,
            Capabilities = new[]
            {
                "Supervise chat agents and operations",
                "Handle complex customer escalations",
                "Monitor chat performance and quality",
                "Manage agent schedules and assignments",
                "Review and approve agent actions",
                "Generate performance reports",
                "Train and mentor chat agents",
                "Escalate to management when needed"
            }
        },

        [RoleName.EndUser] = new RoleInfo
        {
            Name = RoleName.EndUser,
            DisplayName = "End User",
            Description = "Basic user with limited system access for standard operations",
            IsLegacy = false,
            TenantScoped = true,
            Capabilities = new[]
            {
                "Submit and track own issues",
                "View personal dashboard",
                "Update personal profile",
                "Access basic documentation",
                "Receive notifications",
                "Basic reporting access",
                "Standard user operations"
            }
        },

        [RoleName.ApiConsumer] = new RoleInfo
        {
            Name = RoleName.ApiConsumer,
            DisplayName = "API Consumer",
            Description = "External system integration access with limited API permissions",
            IsLegacy = false,
            TenantScoped = true,
            Capabilities = new[]
            {
                "API authentication and access",
                "Read-only data access via API",
                "Submit issues via API integration",
                "Webhook and event notifications",
                "Rate-limited API operations",
                "Integration monitoring",
                "Automated system interactions"
            }
        }
    };

    /// <summary>
    /// Get all non-legacy roles for current system use
    /// </summary>
    public static IEnumerable<RoleInfo> ActiveRoles => 
        Roles.Values.Where(r => !r.IsLegacy);

    /// <summary>
    /// Get all legacy roles for migration purposes
    /// </summary>
    public static IEnumerable<RoleInfo> LegacyRoles => 
        Roles.Values.Where(r => r.IsLegacy);

    /// <summary>
    /// Get roles that are tenant-scoped
    /// </summary>
    public static IEnumerable<RoleInfo> TenantScopedRoles => 
        Roles.Values.Where(r => r.TenantScoped);

    /// <summary>
    /// Get roles that work across tenants
    /// </summary>
    public static IEnumerable<RoleInfo> CrossTenantRoles => 
        Roles.Values.Where(r => !r.TenantScoped);
}

/// <summary>
/// Detailed information about a system role
/// </summary>
public class RoleInfo
{
    /// <summary>
    /// Role name constant
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Human-readable display name
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Detailed description of role purpose
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Whether this is a legacy role to be deprecated
    /// </summary>
    public required bool IsLegacy { get; init; }

    /// <summary>
    /// Whether role assignments are scoped to tenants
    /// </summary>
    public required bool TenantScoped { get; init; }

    /// <summary>
    /// List of capabilities this role provides
    /// </summary>
    public required string[] Capabilities { get; init; }
}