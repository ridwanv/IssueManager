// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.Property(x => x.ConversationId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasMaxLength(100);

        builder.Property(x => x.UserName)
            .HasMaxLength(100);

        builder.Property(x => x.WhatsAppPhoneNumber)
            .HasMaxLength(20);

        builder.Property(x => x.CurrentAgentId)
            .HasMaxLength(100);

        builder.Property(x => x.EscalationReason)
            .HasMaxLength(500);

        builder.Property(x => x.ConversationSummary)
            .HasMaxLength(2000);

        builder.Property(x => x.ThreadId)
            .HasMaxLength(100);

        builder.Property(x => x.TenantId)
            .HasMaxLength(50)
            .IsRequired();

        // Indexes for better query performance
        builder.HasIndex(x => x.ConversationId)
            .IsUnique();

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Mode);
        builder.HasIndex(x => x.LastActivityAt);
        builder.HasIndex(x => new { x.Status, x.Mode });

        // Configure relationships
        builder.HasMany(x => x.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(e => e.DomainEvents);
    }
}