// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using CleanArchitecture.Blazor.Domain.Common.Entities;

namespace CleanArchitecture.Blazor.Domain.Entities
{
    public class InternalNote : BaseAuditableEntity, IMustHaveTenant
    {
        public new Guid Id { get; set; }
        public Guid IssueId { get; set; }
        public Issue Issue { get; set; } = default!;
        public string Content { get; set; } = default!;
        public string CreatedByUserId { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public string TenantId { get; set; } = default!;
    }
}