// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Common.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Domain.Entities
{
    public class IssueLink : BaseAuditableEntity, IMustHaveTenant
    {
        public new Guid Id { get; set; }
        
        /// <summary>
        /// The parent/primary issue ID
        /// </summary>
        public Guid ParentIssueId { get; set; }
        
        /// <summary>
        /// The child/linked issue ID
        /// </summary>
        public Guid ChildIssueId { get; set; }
        
        /// <summary>
        /// Type of relationship between issues
        /// </summary>
        public IssueLinkType LinkType { get; set; }
        
        /// <summary>
        /// Confidence score from similarity detection (0.0 to 1.0)
        /// Null for manually created links
        /// </summary>
        public decimal? ConfidenceScore { get; set; }
        
        /// <summary>
        /// Whether this link was created automatically by the system
        /// </summary>
        public bool CreatedBySystem { get; set; }
        
        /// <summary>
        /// Additional metadata about the link (JSON)
        /// Can store OpenAI analysis results, manual linking reasons, etc.
        /// </summary>
        public string? Metadata { get; set; }
        
        public string TenantId { get; set; } = default!;

        // Navigation properties
        public Issue ParentIssue { get; set; } = default!;
        public Issue ChildIssue { get; set; } = default!;

        /// <summary>
        /// Factory method to create an automatic system link
        /// </summary>
        public static IssueLink CreateSystemLink(
            Guid parentIssueId,
            Guid childIssueId,
            IssueLinkType linkType,
            decimal confidenceScore,
            string tenantId,
            string? metadata = null)
        {
            return new IssueLink
            {
                Id = Guid.NewGuid(),
                ParentIssueId = parentIssueId,
                ChildIssueId = childIssueId,
                LinkType = linkType,
                ConfidenceScore = confidenceScore,
                CreatedBySystem = true,
                Metadata = metadata,
                TenantId = tenantId
            };
        }

        /// <summary>
        /// Factory method to create a manual link
        /// </summary>
        public static IssueLink CreateManualLink(
            Guid parentIssueId,
            Guid childIssueId,
            IssueLinkType linkType,
            string tenantId,
            string? metadata = null)
        {
            return new IssueLink
            {
                Id = Guid.NewGuid(),
                ParentIssueId = parentIssueId,
                ChildIssueId = childIssueId,
                LinkType = linkType,
                ConfidenceScore = null,
                CreatedBySystem = false,
                Metadata = metadata,
                TenantId = tenantId
            };
        }
    }
}