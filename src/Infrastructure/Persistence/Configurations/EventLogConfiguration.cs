// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CleanArchitecture.Blazor.Domain.Entities;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class EventLogConfiguration : IEntityTypeConfiguration<EventLog>
{
    public void Configure(EntityTypeBuilder<EventLog> builder)
    {
        builder.ToTable("EventLogs");
        
        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .IsRequired();
            
        builder.Property(e => e.IssueId)
            .HasColumnName("IssueId")
            .IsRequired();
            
        builder.Property(e => e.Type)
            .HasColumnName("Type")
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(e => e.Payload)
            .HasColumnName("Payload")
            .IsRequired();
            
        builder.Property(e => e.CreatedUtc)
            .HasColumnName("CreatedUtc")
            .IsRequired();
            
        builder.Property(e => e.TenantId)
            .HasColumnName("TenantId")
            .IsRequired()
            .HasMaxLength(450);

        // Relationships
        builder.HasOne(e => e.Issue)
            .WithMany(i => i.EventLogs)
            .HasForeignKey(e => e.IssueId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.IssueId)
            .HasDatabaseName("IX_EventLogs_IssueId");
            
        builder.HasIndex(e => e.TenantId)
            .HasDatabaseName("IX_EventLogs_TenantId");
            
        builder.HasIndex(e => new { e.IssueId, e.CreatedUtc })
            .HasDatabaseName("IX_EventLogs_IssueId_CreatedUtc");
            
        builder.HasIndex(e => e.Type)
            .HasDatabaseName("IX_EventLogs_Type");
    }
}