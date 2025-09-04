namespace CleanArchitecture.Blazor.Domain.Events;

public class IssueCreatedEvent : DomainEvent
{
    public IssueCreatedEvent(Issue item)
    {
        Item = item;
    }

    public Issue Item { get; }
}