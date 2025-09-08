// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessage>
{
    public void Configure(EntityTypeBuilder<ConversationMessage> builder)
    {
        builder.Property(x => x.BotFrameworkConversationId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Role)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Content)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(x => x.ToolCallId)
            .HasMaxLength(100);

        builder.Property(x => x.ToolCalls)
            .HasMaxLength(2000);

        builder.Property(x => x.ImageType)
            .HasMaxLength(50);

        builder.Property(x => x.ImageData)
            .HasColumnType("TEXT"); // SQLite uses TEXT for large text data

        builder.Property(x => x.Attachments)
            .HasMaxLength(2000);

        builder.Property(x => x.UserId)
            .HasMaxLength(100);

        builder.Property(x => x.UserName)
            .HasMaxLength(100);

        builder.Property(x => x.ChannelId)
            .HasMaxLength(50);

        builder.Property(x => x.Timestamp)
            .IsRequired();

        builder.Property(x => x.TenantId)
            .HasMaxLength(50)
            .IsRequired();

        // Indexes for better query performance
        builder.HasIndex(x => x.BotFrameworkConversationId);
        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => x.Timestamp);
        builder.HasIndex(x => new { x.BotFrameworkConversationId, x.Timestamp });

        // Foreign key relationships
        builder.HasOne(x => x.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.AttachmentEntities)
            .WithOne(a => a.Message)
            .HasForeignKey(a => a.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(e => e.DomainEvents);
    }
}
