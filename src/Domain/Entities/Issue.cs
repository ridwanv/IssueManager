// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using CleanArchitecture.Blazor.Domain.Common.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using CleanArchitecture.Blazor.Domain.Events;

namespace CleanArchitecture.Blazor.Domain.Entities
{
    public class Issue : BaseAuditableEntity, IMustHaveTenant
    {
        public new Guid Id { get; set; }
        public string ReferenceNumber { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public IssueCategory Category { get; set; }
        public IssuePriority Priority { get; set; }
        public IssueStatus Status { get; set; } = IssueStatus.New;
        
        // Contact information
        public int? ReporterContactId { get; set; }
        public Contact? ReporterContact { get; set; }
        
        // WhatsApp specific metadata
        public string? SourceMessageIds { get; set; } // JSON array as string
        public string? WhatsAppMetadata { get; set; } // JSON metadata for conversation state
        public bool ConsentFlag { get; set; }
        
        // Legacy fields for backward compatibility
        public string? ReporterPhone { get; set; }
        public string? ReporterName { get; set; }
        public string? Channel { get; set; }
        public string? Product { get; set; }
        public string? Severity { get; set; }
        public string? Summary { get; set; }
        
        // Relationships
        public Guid? DuplicateOfId { get; set; }
        public Issue? DuplicateOf { get; set; }
        public string TenantId { get; set; } = default!;
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public ICollection<EventLog> EventLogs { get; set; } = new List<EventLog>();

        /// <summary>
        /// Factory method to create a new Issue from WhatsApp intake data
        /// </summary>
        public static Issue Create(
            string referenceNumber,
            string title,
            string description,
            IssueCategory category,
            IssuePriority priority,
            int? reporterContactId,
            string tenantId,
            string? sourceMessageIds = null,
            string? whatsAppMetadata = null,
            bool consentFlag = true)
        {
            var issue = new Issue
            {
                Id = Guid.NewGuid(),
                ReferenceNumber = referenceNumber,
                Title = title,
                Description = description,
                Category = category,
                Priority = priority,
                Status = IssueStatus.New,
                ReporterContactId = reporterContactId,
                SourceMessageIds = sourceMessageIds ?? "[]",
                WhatsAppMetadata = whatsAppMetadata,
                ConsentFlag = consentFlag,
                TenantId = tenantId,
                Channel = "WhatsApp" // Default for WhatsApp created issues
            };
            
            // Add domain event for issue creation
            issue.AddDomainEvent(new IssueCreatedEvent(issue));
            
            return issue;
        }
    }
}