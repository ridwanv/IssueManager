using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Issues.DTOs;
using CleanArchitecture.Blazor.Domain.Events;
using CleanArchitecture.Blazor.Application.Features.Issues.Caching;
using System.ComponentModel;

namespace CleanArchitecture.Blazor.Application.Features.Issues.Commands.UpdateIssue;

public class UpdateIssueCommand : ICacheInvalidatorRequest<Result<Guid>>
{
    [Description("Issue ID for direct updates")]
    public Guid? Id { get; set; }

    [Description("JIRA key for webhook-based updates")]
    public string? JiraKey { get; set; }

    [Description("Issue title/summary")]
    public string? Summary { get; set; }

    [Description("Issue description")]
    public string? Description { get; set; }

    [Description("Issue status")]
    public string? Status { get; set; }

    [Description("Issue priority")]
    public string? Priority { get; set; }

    [Description("Assigned user")]
    public string? Assignee { get; set; }

    [Description("List of changed fields from JIRA")]
    public List<string>? ChangedFields { get; set; }

    [Description("Last sync timestamp with JIRA")]
    public DateTime? JiraLastSyncAt { get; set; }

    public string CacheKey => IssueCacheKey.GetAllCacheKey;
    public IEnumerable<string>? Tags => IssueCacheKey.Tags;
}
