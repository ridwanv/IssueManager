// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CleanArchitecture.Blazor.Domain.Entities;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

#nullable disable
public class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        builder.Property(t => t.ApplicationUserId).HasMaxLength(450).IsRequired();
        builder.Property(t => t.Skills).HasMaxLength(1000);
        builder.Property(t => t.Notes).HasMaxLength(2000);
        
        builder.HasIndex(t => t.ApplicationUserId).IsUnique();
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Priority);
        
        builder.HasOne(a => a.ApplicationUser)
            .WithMany(u => u.Agents)
            .HasForeignKey(a => a.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Note: Conversations are linked via CurrentAgentId to ApplicationUserId, not Agent.Id
        
        builder.Ignore(e => e.DomainEvents);
    }
}