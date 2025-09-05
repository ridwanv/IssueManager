using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Domain.Events;

public class IssueUpdatedFromJiraEvent : DomainEvent
{
    public IssueUpdatedFromJiraEvent(
        Guid issueId,
        string jiraKey,
        List<string> localUpdatedFields,
        List<string> jiraChangedFields,
        IssueStatus currentStatus,
        string? reporterPhone)
    {
        IssueId = issueId;
        JiraKey = jiraKey;
        LocalUpdatedFields = localUpdatedFields;
        JiraChangedFields = jiraChangedFields;
        CurrentStatus = currentStatus;
        ReporterPhone = reporterPhone;
    }

    public Guid IssueId { get; }
    public string JiraKey { get; }
    public List<string> LocalUpdatedFields { get; }
    public List<string> JiraChangedFields { get; }
    public IssueStatus CurrentStatus { get; }
    public string? ReporterPhone { get; }
}
