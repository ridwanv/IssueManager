using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Domain.Events;

public class IssueStatusChangedEvent : DomainEvent
{
    public IssueStatusChangedEvent(Issue item, IssueStatus previousStatus, IssueStatus newStatus)
    {
        Item = item;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
    }

    public Issue Item { get; }
    public IssueStatus PreviousStatus { get; }
    public IssueStatus NewStatus { get; }
}