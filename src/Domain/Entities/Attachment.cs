using System;
using CleanArchitecture.Blazor.Domain.Common.Entities;

namespace CleanArchitecture.Blazor.Domain.Entities
{
    public class Attachment : BaseAuditableEntity, IMustHaveTenant
    {
        public new Guid Id { get; set; }
        public Guid IssueId { get; set; }
        public Issue Issue { get; set; } = default!;
        public string Uri { get; set; } = default!;
        public string Type { get; set; } = default!;
        public long SizeBytes { get; set; }
        public string ScanStatus { get; set; } = default!;
        public DateTime CreatedUtc { get; set; }
        public string TenantId { get; set; } = default!;
    }
}