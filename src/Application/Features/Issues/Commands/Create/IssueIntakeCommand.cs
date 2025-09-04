using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Issues.Caching;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Events;

namespace CleanArchitecture.Blazor.Application.Features.Issues.Commands.Create;

public record IssueIntakeCommand : ICacheInvalidatorRequest<Result<Guid>>
{
    public required string ReporterPhone { get; set; }
    public string? ReporterName { get; set; }
    public required string Channel { get; set; }
    public required string Category { get; set; }
    public required string Product { get; set; }
    public required string Severity { get; set; }
    public required string Priority { get; set; }
    public required string Summary { get; set; }
    public required string Description { get; set; }
    public string? SourceMessageIds { get; set; }
    public bool ConsentFlag { get; set; } = true;
    public string Status { get; set; } = "New";
    public Guid? ContactId { get; set; }
    public List<IssueAttachmentData>? Attachments { get; set; }

    public string CacheKey => IssueCacheKey.GetAllCacheKey;
    public IEnumerable<string>? Tags => IssueCacheKey.Tags;
    public CancellationToken CancellationToken { get; set; }
}

public record IssueAttachmentData
{
    public required string Name { get; set; }
    public required string ContentType { get; set; }
    public required string Url { get; set; }
    public long Size { get; set; }
}