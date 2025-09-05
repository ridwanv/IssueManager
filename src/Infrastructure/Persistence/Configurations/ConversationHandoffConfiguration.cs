// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CleanArchitecture.Blazor.Domain.Entities;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

#nullable disable
public class ConversationHandoffConfiguration : IEntityTypeConfiguration<ConversationHandoff>
{
    public void Configure(EntityTypeBuilder<ConversationHandoff> builder)
    {
        builder.Property(t => t.FromAgentId).HasMaxLength(450);
        builder.Property(t => t.ToAgentId).HasMaxLength(450);
        builder.Property(t => t.Reason).HasMaxLength(500).IsRequired();
        builder.Property(t => t.ConversationTranscript);
        builder.Property(t => t.ContextData).HasMaxLength(2000);
        builder.Property(t => t.Notes).HasMaxLength(1000);
        
        builder.HasIndex(t => t.ConversationId);
        builder.HasIndex(t => t.HandoffType);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.FromAgentId);
        builder.HasIndex(t => t.ToAgentId);
        builder.HasIndex(t => t.InitiatedAt);
        
        builder.HasOne(h => h.Conversation)
            .WithMany(c => c.Handoffs)
            .HasForeignKey(h => h.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Ignore(e => e.DomainEvents);
    }
}