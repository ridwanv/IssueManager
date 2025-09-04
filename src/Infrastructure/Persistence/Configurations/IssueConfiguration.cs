// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

#nullable disable
public class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        builder.HasKey(e => e.Id);
        
        // Reference number should be unique
        builder.Property(e => e.ReferenceNumber)
            .HasMaxLength(20)
            .IsRequired();
        builder.HasIndex(e => e.ReferenceNumber)
            .IsUnique();
            
        builder.Property(e => e.Title)
            .HasMaxLength(200)
            .IsRequired();
            
        builder.Property(e => e.Description)
            .HasMaxLength(2000)
            .IsRequired();
            
        // Configure enums as integers
        builder.Property(e => e.Category)
            .HasConversion<int>()
            .IsRequired();
            
        builder.Property(e => e.Priority)
            .HasConversion<int>()
            .IsRequired();
            
        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();
        
        // WhatsApp metadata stored as JSON
        builder.Property(e => e.WhatsAppMetadata)
            .HasColumnType("nvarchar(max)");
            
        builder.Property(e => e.SourceMessageIds)
            .HasColumnType("nvarchar(max)");
        
        // Legacy fields
        builder.Property(e => e.ReporterPhone)
            .HasMaxLength(50);
            
        builder.Property(e => e.ReporterName)
            .HasMaxLength(100);
            
        builder.Property(e => e.Channel)
            .HasMaxLength(50);
            
        builder.Property(e => e.Product)
            .HasMaxLength(100);
            
        builder.Property(e => e.Severity)
            .HasMaxLength(50);
            
        builder.Property(e => e.Summary)
            .HasMaxLength(500);
        
        // Configure relationships
        builder.HasOne(e => e.ReporterContact)
            .WithMany()
            .HasForeignKey(e => e.ReporterContactId)
            .OnDelete(DeleteBehavior.SetNull);
            
        builder.HasOne(e => e.DuplicateOf)
            .WithMany()
            .HasForeignKey(e => e.DuplicateOfId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Configure collections
        builder.HasMany(e => e.Attachments)
            .WithOne()
            .HasForeignKey(a => a.IssueId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(e => e.EventLogs)
            .WithOne()
            .HasForeignKey(el => el.IssueId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Tenant isolation
        builder.Property(e => e.TenantId)
            .HasMaxLength(450)
            .IsRequired();
            
        builder.HasIndex(e => e.TenantId);
        
        // Ignore domain events
        builder.Ignore(e => e.DomainEvents);
    }
}
