using CleanArchitecture.Blazor.Domain.Entities;

namespace CleanArchitecture.Blazor.Domain.Events;

public class IssueResolvedEvent : DomainEvent
{
    public IssueResolvedEvent(Issue item, string? resolutionNotes = null)
    {
        Item = item;
        ResolutionNotes = resolutionNotes;
    }

    public Issue Item { get; }
    public string? ResolutionNotes { get; }
}