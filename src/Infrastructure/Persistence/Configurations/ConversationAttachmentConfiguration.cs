// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class ConversationAttachmentConfiguration : IEntityTypeConfiguration<ConversationAttachment>
{
    public void Configure(EntityTypeBuilder<ConversationAttachment> builder)
    {
        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasMaxLength(100);

        builder.Property(x => x.Url)
            .HasMaxLength(500);

        builder.Property(x => x.TenantId)
            .HasMaxLength(50)
            .IsRequired();

        // FileData can be large, no max length restriction for binary data

        // Indexes for better query performance
        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => x.MessageId);
        builder.HasIndex(x => x.ContentType);

        // Configure relationships
        builder.HasOne(x => x.Conversation)
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Message)
            .WithMany(m => m.AttachmentEntities)
            .HasForeignKey(x => x.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(e => e.DomainEvents);
    }
}
