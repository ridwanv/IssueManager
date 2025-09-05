// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CleanArchitecture.Blazor.Domain.Entities;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

#nullable disable
public class ConversationParticipantConfiguration : IEntityTypeConfiguration<ConversationParticipant>
{
    public void Configure(EntityTypeBuilder<ConversationParticipant> builder)
    {
        builder.Property(t => t.ParticipantId).HasMaxLength(450);
        builder.Property(t => t.ParticipantName).HasMaxLength(200);
        builder.Property(t => t.WhatsAppPhoneNumber).HasMaxLength(20);
        
        builder.HasIndex(t => t.ConversationId);
        builder.HasIndex(t => t.Type);
        builder.HasIndex(t => t.ParticipantId);
        builder.HasIndex(t => t.WhatsAppPhoneNumber);
        
        builder.HasOne(p => p.Conversation)
            .WithMany(c => c.Participants)
            .HasForeignKey(p => p.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Ignore(e => e.DomainEvents);
    }
}