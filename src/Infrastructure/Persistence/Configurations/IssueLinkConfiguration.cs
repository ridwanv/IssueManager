// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

#nullable disable
public class IssueLinkConfiguration : IEntityTypeConfiguration<IssueLink>
{
    public void Configure(EntityTypeBuilder<IssueLink> builder)
    {
        builder.HasKey(e => e.Id);
        
        // Configure link type as integer
        builder.Property(e => e.LinkType)
            .HasConversion<int>()
            .IsRequired();
            
        // Configure confidence score with precision
        builder.Property(e => e.ConfidenceScore)
            .HasColumnType("decimal(5,4)"); // Supports 0.0000 to 1.0000
            
        builder.Property(e => e.CreatedBySystem)
            .IsRequired()
            .HasDefaultValue(false);
            
        // Metadata can store JSON data about the link
        builder.Property(e => e.Metadata)
            .HasMaxLength(1000);
        
        // Configure parent relationship
        builder.HasOne(e => e.ParentIssue)
            .WithMany(i => i.ChildLinks)
            .HasForeignKey(e => e.ParentIssueId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Configure child relationship
        builder.HasOne(e => e.ChildIssue)
            .WithMany(i => i.ParentLinks)
            .HasForeignKey(e => e.ChildIssueId)
            .OnDelete(DeleteBehavior.ClientCascade); // Prevent circular cascade
        
        // Tenant isolation
        builder.Property(e => e.TenantId)
            .HasMaxLength(450)
            .IsRequired();
            
        builder.HasIndex(e => e.TenantId);
        
        // Performance indices
        builder.HasIndex(e => e.ParentIssueId);
        builder.HasIndex(e => e.ChildIssueId);
        builder.HasIndex(e => new { e.ParentIssueId, e.ChildIssueId, e.TenantId })
            .IsUnique(); // Prevent duplicate links
        builder.HasIndex(e => new { e.LinkType, e.TenantId });
        builder.HasIndex(e => new { e.CreatedBySystem, e.TenantId });
        builder.HasIndex(e => new { e.ConfidenceScore, e.TenantId });
        
        // Ignore domain events
        builder.Ignore(e => e.DomainEvents);
    }
}