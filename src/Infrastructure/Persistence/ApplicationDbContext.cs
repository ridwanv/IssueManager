// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using CleanArchitecture.Blazor.Domain.Common.Entities;
using CleanArchitecture.Blazor.Domain.Identity;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence;

#nullable disable
public class ApplicationDbContext : IdentityDbContext<
    ApplicationUser, ApplicationRole, string,
    ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin,
    ApplicationRoleClaim, ApplicationUserToken>, IApplicationDbContext, IDataProtectionKeyContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<SystemLog> SystemLogs { get; set; }
    public DbSet<AuditTrail> AuditTrails { get; set; }
    public DbSet<Document> Documents { get; set; }

    public DbSet<PicklistSet> PicklistSets { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Issue> Issues { get; set; }
    public DbSet<IssueLink> IssueLinks { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<EventLog> EventLogs { get; set; }
    public DbSet<InternalNote> InternalNotes { get; set; }
    public DbSet<LoginAudit> LoginAudits { get; set; }
    public DbSet<UserLoginRiskSummary> UserLoginRiskSummaries { get; set; }
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    
    // Conversation escalation entities
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationMessage> ConversationMessages { get; set; }
    public DbSet<ConversationAttachment> ConversationAttachments { get; set; }
    public DbSet<Agent> Agents { get; set; }
    public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
    public DbSet<ConversationHandoff> ConversationHandoffs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.ApplyGlobalFilters<ISoftDelete>(s => s.Deleted == null);
    }
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<string>().HaveMaxLength(450);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }
}