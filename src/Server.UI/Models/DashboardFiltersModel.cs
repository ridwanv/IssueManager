namespace CleanArchitecture.Blazor.Server.UI.Models;
using CleanArchitecture.Blazor.Domain.Enums;

public record DashboardFiltersModel
{
    public string TimeRange { get; set; } = "30d";
    public IssueStatus? Status { get; set; }
    public IssuePriority? Priority { get; set; }
    public IssueCategory? Category { get; set; }
    public string? Channel { get; set; }
    public bool AutoRefresh { get; set; } = true;
}
