// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace CleanArchitecture.Blazor.Application.Common.Security;

/// <summary>
/// Defines all persona-based permissions for the SaaS platform
/// </summary>
public static partial class Permissions
{
    [DisplayName("Platform Administration Permissions")]
    [Description("Set permissions for platform-level administration")]
    public static class Platform
    {
        [Description("View all tenants across the platform")]
        public const string ViewAllTenants = "Permissions.Platform.ViewAllTenants";
        [Description("Create new tenants")]
        public const string CreateTenants = "Permissions.Platform.CreateTenants";
        [Description("Modify existing tenants")]
        public const string ManageTenants = "Permissions.Platform.ManageTenants";
        [Description("Delete tenants")]
        public const string DeleteTenants = "Permissions.Platform.DeleteTenants";
        [Description("View system health and performance metrics")]
        public const string ViewSystemHealth = "Permissions.Platform.ViewSystemHealth";
        [Description("Manage system configuration and settings")]
        public const string ManageSystem = "Permissions.Platform.ManageSystem";
        [Description("Access platform-level analytics")]
        public const string ViewAnalytics = "Permissions.Platform.ViewAnalytics";
        [Description("Manage platform billing and subscriptions")]
        public const string ManageBilling = "Permissions.Platform.ManageBilling";
        [Description("Switch between tenant contexts")]
        public const string SwitchTenants = "Permissions.Platform.SwitchTenants";
    }

    [DisplayName("Tenant Management Permissions")]
    [Description("Set permissions for tenant-level management")]
    public static class Tenant
    {
        [Description("View tenant user list")]
        public const string ViewUsers = "Permissions.Tenant.ViewUsers";
        [Description("Add, remove, and modify tenant users")]
        public const string ManageUsers = "Permissions.Tenant.ManageUsers";
        [Description("View tenant configuration and settings")]
        public const string ViewSettings = "Permissions.Tenant.ViewSettings";
        [Description("Modify tenant settings and configuration")]
        public const string ManageSettings = "Permissions.Tenant.ManageSettings";
        [Description("View billing and subscription information")]
        public const string ViewBilling = "Permissions.Tenant.ViewBilling";
        [Description("Modify subscriptions and payment methods")]
        public const string ManageBilling = "Permissions.Tenant.ManageBilling";
        [Description("Assign and modify user roles")]
        public const string ManageRoles = "Permissions.Tenant.ManageRoles";
        [Description("View tenant analytics and reports")]
        public const string ViewAnalytics = "Permissions.Tenant.ViewAnalytics";
    }

    [DisplayName("Chat Agent Permissions")]
    [Description("Set permissions for human chat agents")]
    public static class ChatAgent
    {
        [Description("Receive escalated conversations")]
        public const string ReceiveEscalations = "Permissions.ChatAgent.ReceiveEscalations";
        [Description("Send responses to customers")]
        public const string RespondToCustomers = "Permissions.ChatAgent.RespondToCustomers";
        [Description("Transfer conversations to other agents")]
        public const string TransferConversations = "Permissions.ChatAgent.TransferConversations";
        [Description("Access conversation history")]
        public const string ViewConversationHistory = "Permissions.ChatAgent.ViewConversationHistory";
        [Description("Create issues from conversations")]
        public const string CreateIssuesFromConversations = "Permissions.ChatAgent.CreateIssuesFromConversations";
        [Description("Update agent status and availability")]
        public const string ManageStatus = "Permissions.ChatAgent.ManageStatus";
        [Description("View assigned conversation queue")]
        public const string ViewQueue = "Permissions.ChatAgent.ViewQueue";
        [Description("Close and wrap up conversations")]
        public const string CloseConversations = "Permissions.ChatAgent.CloseConversations";
    }

    [DisplayName("Chat Supervisor Permissions")]  
    [Description("Set permissions for chat supervisors")]
    public static class ChatSupervisor
    {
        [Description("Assign escalated conversations to agents")]
        public const string AssignConversations = "Permissions.ChatSupervisor.AssignConversations";
        [Description("Monitor agent response times and performance")]
        public const string MonitorPerformance = "Permissions.ChatSupervisor.MonitorPerformance";
        [Description("Handle complex escalations beyond agent authority")]
        public const string HandleComplexEscalations = "Permissions.ChatSupervisor.HandleComplexEscalations";
        [Description("View all agent queues and workloads")]
        public const string ViewAllQueues = "Permissions.ChatSupervisor.ViewAllQueues";
        [Description("Manage agent assignments and schedules")]
        public const string ManageAgentAssignments = "Permissions.ChatSupervisor.ManageAgentAssignments";
        [Description("Override agent status and availability")]
        public const string OverrideAgentStatus = "Permissions.ChatSupervisor.OverrideAgentStatus";
        [Description("View comprehensive chat analytics")]
        public const string ViewChatAnalytics = "Permissions.ChatSupervisor.ViewChatAnalytics";
    }

    [DisplayName("Issue Manager Permissions")]
    [Description("Set permissions for issue managers")]
    public static class IssueManager
    {
        [Description("Review and validate auto-created issues")]
        public const string ReviewAutoCreatedIssues = "Permissions.IssueManager.ReviewAutoCreatedIssues";
        [Description("Assign issues to team members")]
        public const string AssignIssues = "Permissions.IssueManager.AssignIssues";
        [Description("Track issue progress and resolution")]
        public const string TrackIssueProgress = "Permissions.IssueManager.TrackIssueProgress";
        [Description("View all issues within tenant")]
        public const string ViewAllIssues = "Permissions.IssueManager.ViewAllIssues";
        [Description("Modify issue priorities and categories")]
        public const string ModifyIssuePriorities = "Permissions.IssueManager.ModifyIssuePriorities";
        [Description("Close and resolve issues")]
        public const string CloseIssues = "Permissions.IssueManager.CloseIssues";
        [Description("View issue analytics and reports")]
        public const string ViewIssueAnalytics = "Permissions.IssueManager.ViewIssueAnalytics";
    }

    [DisplayName("Issue Assignee Permissions")]
    [Description("Set permissions for issue assignees")]
    public static class IssueAssignee
    {
        [Description("View assigned issues")]
        public const string ViewAssignedIssues = "Permissions.IssueAssignee.ViewAssignedIssues";
        [Description("Update issue status and progress")]
        public const string UpdateIssueStatus = "Permissions.IssueAssignee.UpdateIssueStatus";
        [Description("Add notes and comments to issues")]
        public const string AddIssueNotes = "Permissions.IssueAssignee.AddIssueNotes";
        [Description("Request issue reassignment")]
        public const string RequestReassignment = "Permissions.IssueAssignee.RequestReassignment";
        [Description("Escalate complex issues")]
        public const string EscalateIssues = "Permissions.IssueAssignee.EscalateIssues";
        [Description("View external tool integration status")]
        public const string ViewIntegrationStatus = "Permissions.IssueAssignee.ViewIntegrationStatus";
    }

    [DisplayName("Issue Supervisor Permissions")]
    [Description("Set permissions for issue supervisors")]
    public static class IssueSupervisor
    {
        [Description("Monitor issue resolution metrics and SLAs")]
        public const string MonitorMetrics = "Permissions.IssueSupervisor.MonitorMetrics";
        [Description("Handle escalations from assignees")]
        public const string HandleEscalations = "Permissions.IssueSupervisor.HandleEscalations";
        [Description("Manage team workload and resource allocation")]
        public const string ManageWorkload = "Permissions.IssueSupervisor.ManageWorkload";
        [Description("Override issue assignments")]
        public const string OverrideAssignments = "Permissions.IssueSupervisor.OverrideAssignments";
        [Description("View comprehensive issue analytics")]
        public const string ViewComprehensiveAnalytics = "Permissions.IssueSupervisor.ViewComprehensiveAnalytics";
        [Description("Manage issue resolution SLAs")]
        public const string ManageSLAs = "Permissions.IssueSupervisor.ManageSLAs";
    }

    [DisplayName("JIRA Integrator Permissions")]
    [Description("Set permissions for JIRA integrators")]
    public static class JiraIntegrator
    {
        [Description("Configure JIRA integration settings")]
        public const string ConfigureIntegration = "Permissions.JiraIntegrator.ConfigureIntegration";
        [Description("Maintain JIRA synchronization")]
        public const string ManageSync = "Permissions.JiraIntegrator.ManageSync";
        [Description("Resolve sync conflicts")]
        public const string ResolveSyncConflicts = "Permissions.JiraIntegrator.ResolveSyncConflicts";
        [Description("Map fields between systems")]
        public const string ManageFieldMapping = "Permissions.JiraIntegrator.ManageFieldMapping";
        [Description("View integration logs and status")]
        public const string ViewIntegrationLogs = "Permissions.JiraIntegrator.ViewIntegrationLogs";
    }

    [DisplayName("Bot Management Permissions")]
    [Description("Set permissions for managing automated bots")]
    public static class BotManagement
    {
        [Description("Configure conversation bot settings")]
        public const string ConfigureConversationBots = "Permissions.BotManagement.ConfigureConversationBots";
        [Description("Configure issue logger bot settings")]
        public const string ConfigureIssueLoggerBots = "Permissions.BotManagement.ConfigureIssueLoggerBots";
        [Description("View bot performance metrics")]
        public const string ViewBotMetrics = "Permissions.BotManagement.ViewBotMetrics";
        [Description("Manage bot escalation rules")]
        public const string ManageEscalationRules = "Permissions.BotManagement.ManageEscalationRules";
        [Description("Override bot decisions")]
        public const string OverrideBotDecisions = "Permissions.BotManagement.OverrideBotDecisions";
    }

    [DisplayName("End User Permissions")]
    [Description("Set permissions for end users")]
    public static class EndUser
    {
        [Description("Submit new issues")]
        public const string SubmitIssues = "Permissions.EndUser.SubmitIssues";
        [Description("Track submitted issues")]
        public const string TrackIssues = "Permissions.EndUser.TrackIssues";
        [Description("Initiate chat support requests")]
        public const string InitiateChat = "Permissions.EndUser.InitiateChat";
        [Description("View issue status updates")]
        public const string ViewStatusUpdates = "Permissions.EndUser.ViewStatusUpdates";
        [Description("Receive notifications")]
        public const string ReceiveNotifications = "Permissions.EndUser.ReceiveNotifications";
        [Description("Provide feedback on resolutions")]
        public const string ProvideFeedback = "Permissions.EndUser.ProvideFeedback";
    }

    [DisplayName("API Consumer Permissions")]
    [Description("Set permissions for API consumers and integrations")]
    public static class ApiConsumer
    {
        [Description("Access issues via API")]
        public const string AccessIssuesApi = "Permissions.ApiConsumer.AccessIssuesApi";
        [Description("Access conversations via API")]
        public const string AccessConversationsApi = "Permissions.ApiConsumer.AccessConversationsApi";
        [Description("Create issues via API")]
        public const string CreateIssuesApi = "Permissions.ApiConsumer.CreateIssuesApi";
        [Description("Update issues via API")]
        public const string UpdateIssuesApi = "Permissions.ApiConsumer.UpdateIssuesApi";
        [Description("Access analytics via API")]
        public const string AccessAnalyticsApi = "Permissions.ApiConsumer.AccessAnalyticsApi";
        [Description("Webhook access for real-time updates")]
        public const string WebhookAccess = "Permissions.ApiConsumer.WebhookAccess";
    }
}