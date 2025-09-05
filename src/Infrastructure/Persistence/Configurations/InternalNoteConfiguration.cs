// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CleanArchitecture.Blazor.Domain.Entities;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class InternalNoteConfiguration : IEntityTypeConfiguration<InternalNote>
{
    public void Configure(EntityTypeBuilder<InternalNote> builder)
    {
        builder.ToTable("InternalNotes");
        
        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .IsRequired();
            
        builder.Property(e => e.IssueId)
            .HasColumnName("IssueId")
            .IsRequired();
            
        builder.Property(e => e.Content)
            .HasColumnName("Content")
            .IsRequired()
            .HasMaxLength(2000);
            
        builder.Property(e => e.CreatedByUserId)
            .HasColumnName("CreatedByUserId")
            .IsRequired()
            .HasMaxLength(450);
            
        builder.Property(e => e.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();
            
        builder.Property(e => e.TenantId)
            .HasColumnName("TenantId")
            .IsRequired()
            .HasMaxLength(450);

        // Relationships
        builder.HasOne(e => e.Issue)
            .WithMany()
            .HasForeignKey(e => e.IssueId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.IssueId)
            .HasDatabaseName("IX_InternalNotes_IssueId");
            
        builder.HasIndex(e => e.TenantId)
            .HasDatabaseName("IX_InternalNotes_TenantId");
            
        builder.HasIndex(e => new { e.IssueId, e.CreatedAt })
            .HasDatabaseName("IX_InternalNotes_IssueId_CreatedAt");
    }
}