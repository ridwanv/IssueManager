// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using CleanArchitecture.Blazor.Domain.Identity;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CleanArchitecture.Blazor.Application.Common.Interfaces;

public interface IApplicationDbContext: IAsyncDisposable
{
    DbSet<SystemLog> SystemLogs { get; set; }
    DbSet<AuditTrail> AuditTrails { get; set; }
    DbSet<Document> Documents { get; set; }
    DbSet<PicklistSet> PicklistSets { get; set; }
    DbSet<Product> Products { get; set; }
    DbSet<Tenant> Tenants { get; set; }
    DbSet<Contact> Contacts { get; set; }
    DbSet<Issue> Issues { get; set; }
    DbSet<IssueLink> IssueLinks { get; set; }
    DbSet<Attachment> Attachments { get; set; }
    DbSet<EventLog> EventLogs { get; set; }
    DbSet<InternalNote> InternalNotes { get; set; }
    DbSet<LoginAudit> LoginAudits { get; set; }
    DbSet<UserLoginRiskSummary> UserLoginRiskSummaries { get; set; }
    
    // Identity
    DbSet<ApplicationUser> Users { get; set; }
    
    // Conversation escalation entities
    DbSet<Conversation> Conversations { get; set; }
    DbSet<ConversationMessage> ConversationMessages { get; set; }
    DbSet<ConversationAttachment> ConversationAttachments { get; set; }
    DbSet<Agent> Agents { get; set; }
    DbSet<AgentNotificationPreferences> AgentNotificationPreferences { get; set; }
    DbSet<ConversationParticipant> ConversationParticipants { get; set; }
    DbSet<ConversationHandoff> ConversationHandoffs { get; set; }
    
    ChangeTracker ChangeTracker { get; }

    DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}