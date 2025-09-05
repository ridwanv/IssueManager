using CleanArchitecture.Blazor.Domain.Entities;

namespace CleanArchitecture.Blazor.Domain.Events;

public class IssueAssignedEvent : DomainEvent
{
    public IssueAssignedEvent(Issue item, string? previousAssignedUserId, string? newAssignedUserId)
    {
        Item = item;
        PreviousAssignedUserId = previousAssignedUserId;
        NewAssignedUserId = newAssignedUserId;
    }

    public Issue Item { get; }
    public string? PreviousAssignedUserId { get; }
    public string? NewAssignedUserId { get; }
}