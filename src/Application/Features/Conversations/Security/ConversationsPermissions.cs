// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Application.Features.Conversations.Security;

public static class ConversationsPermissions
{
    [Description("View conversations")]
    public const string View = "Permissions.Conversations.View";
    
    [Description("Escalate conversations")]
    public const string Escalate = "Permissions.Conversations.Escalate";
    
    [Description("Assign agents to conversations")]
    public const string Assign = "Permissions.Conversations.Assign";
    
    [Description("Complete conversations")]
    public const string Complete = "Permissions.Conversations.Complete";
    
    [Description("Manage agents")]
    public const string ManageAgents = "Permissions.Conversations.ManageAgents";
    
    [Description("View agent dashboard")]
    public const string ViewAgentDashboard = "Permissions.Conversations.ViewAgentDashboard";
}