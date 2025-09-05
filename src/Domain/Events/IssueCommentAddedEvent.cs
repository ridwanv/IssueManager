using CleanArchitecture.Blazor.Domain.Entities;

namespace CleanArchitecture.Blazor.Domain.Events;

public class IssueCommentAddedEvent : DomainEvent
{
    public IssueCommentAddedEvent(Issue item, string comment, string addedByUserId)
    {
        Item = item;
        Comment = comment;
        AddedByUserId = addedByUserId;
    }

    public Issue Item { get; }
    public string Comment { get; }
    public string AddedByUserId { get; }
}