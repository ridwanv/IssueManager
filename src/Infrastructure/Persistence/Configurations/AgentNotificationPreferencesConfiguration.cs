// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class AgentNotificationPreferencesConfiguration : IEntityTypeConfiguration<AgentNotificationPreferences>
{
    public void Configure(EntityTypeBuilder<AgentNotificationPreferences> builder)
    {
        builder.Property(x => x.ApplicationUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.TenantId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.CustomSoundUrl).HasMaxLength(500);
        
        builder.HasIndex(x => new { x.ApplicationUserId, x.TenantId }).IsUnique();
        builder.HasIndex(x => x.TenantId);
        
        builder.HasOne(x => x.ApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}