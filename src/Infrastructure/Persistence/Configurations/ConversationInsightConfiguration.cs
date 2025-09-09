// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class ConversationInsightConfiguration : IEntityTypeConfiguration<ConversationInsight>
{
    public void Configure(EntityTypeBuilder<ConversationInsight> builder)
    {
        builder.Property(x => x.SentimentScore)
            .HasPrecision(3, 2) // Allows values from -1.00 to 1.00
            .IsRequired();

        builder.Property(x => x.SentimentLabel)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.KeyThemes)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.CustomerSatisfactionIndicators)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.Recommendations)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.ProcessingModel)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ProcessedAt)
            .IsRequired();

        builder.Property(x => x.ProcessingDuration)
            .IsRequired();

        builder.Property(x => x.TenantId)
            .HasMaxLength(50)
            .IsRequired();

        // Indexes for better query performance
        builder.HasIndex(x => x.ConversationId)
            .IsUnique(); // One insight per conversation

        builder.HasIndex(x => x.ProcessedAt);
        builder.HasIndex(x => x.SentimentScore);
        builder.HasIndex(x => new { x.TenantId, x.ProcessedAt });

        builder.Ignore(e => e.DomainEvents);
    }
}