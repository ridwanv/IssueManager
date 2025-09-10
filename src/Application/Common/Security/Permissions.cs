// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Reflection;

namespace CleanArchitecture.Blazor.Application.Common.Security;

public static partial class Permissions
{
    /// <summary>
    ///     Returns a list of Permissions by scanning all assemblies for Permissions classes.
    /// </summary>
    /// <returns></returns>
    public static List<string> GetRegisteredPermissions()
    {
        var permissions = new List<string>();
        
        // Scan current assembly for all classes named "Permissions" (both in Common and Features)
        var assembly = Assembly.GetExecutingAssembly();
        var permissionClasses = assembly.GetTypes()
            .Where(t => t.Name == "Permissions" && t.IsClass && t.IsAbstract && t.IsSealed)
            .ToList();

        foreach (var permissionClass in permissionClasses)
        {
            foreach (var nestedType in permissionClass.GetNestedTypes())
            {
                var fields = nestedType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                foreach (var field in fields)
                {
                    var propertyValue = field.GetValue(null);
                    if (propertyValue is string permission)
                        permissions.Add(permission);
                }
            }
        }

        return permissions.Distinct().ToList();
    }

    [DisplayName("Navigation Menu Permissions")]
    [Description("Set permissions for navigation menu")]
    public static class NavigationMenu
    {
        [Description("Allows viewing the navigation menu")]
        public const string View = "Permissions.NavigationMenu.View";
    }

    [DisplayName("Hangfire Permissions")]
    [Description("Set permissions for Hangfire dashboard")]
    public static class Hangfire
    {
        [Description("Allows viewing Hangfire dashboard")]
        public const string View = "Permissions.Hangfire.View";
    }

    [DisplayName("Conversations Permissions")]
    [Description("Set permissions for conversation escalation and agent management")]
    public static class Conversations
    {
        [Description("View conversations")]
        public const string View = "Permissions.Conversations.View";
        
        [Description("Escalate conversations to human agents")]
        public const string Escalate = "Permissions.Conversations.Escalate";
        
        [Description("Assign agents to conversations")]
        public const string Assign = "Permissions.Conversations.Assign";
        
        [Description("Complete conversations")]
        public const string Complete = "Permissions.Conversations.Complete";
        
        [Description("Manage agents (create, update, delete)")]
        public const string ManageAgents = "Permissions.Conversations.ManageAgents";
        
        [Description("View agent dashboard")]
        public const string ViewAgentDashboard = "Permissions.Conversations.ViewAgentDashboard";
        
        [Description("Join conversations as agent")]
        public const string JoinAsAgent = "Permissions.Conversations.JoinAsAgent";
        
        [Description("View conversation transcripts")]
        public const string ViewTranscripts = "Permissions.Conversations.ViewTranscripts";
        
        [Description("Export conversation data")]
        public const string Export = "Permissions.Conversations.Export";
        
        [Description("Transfer conversations between agents")]
        public const string Transfer = "Permissions.Conversations.Transfer";
        
        [Description("Manage conversation assignments (supervisor level)")]
        public const string ManageAssignments = "Permissions.Conversations.ManageAssignments";
    }
} 