# Backend Architecture

## Controller Template

```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class IssuesController : ApiControllerBase
{
    public IssuesController(IMediator mediator) : base(mediator) { }

    /// <summary>
    /// Get paginated list of issues with filtering
    /// </summary>
    [HttpGet]
    [MustHavePermission(Permissions.Issues.View)]
    public async Task<IActionResult> GetAsync([FromQuery] GetIssuesQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create new issue
    /// </summary>
    [HttpPost]
    [MustHavePermission(Permissions.Issues.Create)]
    public async Task<IActionResult> CreateAsync(CreateIssueCommand command)
    {
        var result = await Mediator.Send(command);
        
        if (result.Succeeded)
        {
            return CreatedAtAction(nameof(GetByIdAsync), 
                new { id = result.Data }, result.Data);
        }
        
        return BadRequest(result.Messages);
    }

    /// <summary>
    /// Upload attachment to issue
    /// </summary>
    [HttpPost("{id:guid}/attachments")]
    [MustHavePermission(Permissions.Issues.Edit)]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50MB limit
    public async Task<IActionResult> UploadAttachmentAsync(
        Guid id, 
        [FromForm] UploadAttachmentCommand command)
    {
        command.IssueId = id;
        var result = await Mediator.Send(command);
        
        return result.Succeeded ? Ok(result.Data) : BadRequest(result.Messages);
    }
}
```

## Domain Entity Example

```csharp
// Domain entity with Clean Architecture patterns
public class Issue : BaseAuditableEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public IssueCategory Category { get; private set; }
    public IssuePriority Priority { get; private set; }
    public IssueStatus Status { get; private set; }
    
    // Navigation properties
    public Guid ReporterContactId { get; private set; }
    public Contact ReporterContact { get; private set; } = null!;
    
    public Guid? AssignedUserId { get; private set; }
    public ApplicationUser? AssignedUser { get; private set; }
    
    public List<Attachment> Attachments { get; private set; } = new();
    public List<EventLog> EventLogs { get; private set; } = new();
    
    // Domain events
    public static Issue Create(string title, string description, 
        IssueCategory category, IssuePriority priority, Guid reporterContactId)
    {
        var issue = new Issue
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Category = category,
            Priority = priority,
            Status = IssueStatus.New,
            ReporterContactId = reporterContactId
        };
        
        issue.AddDomainEvent(new IssueCreatedDomainEvent(issue));
        return issue;
    }
    
    public void AssignTo(Guid userId)
    {
        var oldAssignee = AssignedUserId;
        AssignedUserId = userId;
        
        AddDomainEvent(new IssueAssignedDomainEvent(Id, oldAssignee, userId));
    }
    
    public void UpdateStatus(IssueStatus newStatus)
    {
        var oldStatus = Status;
        Status = newStatus;
        
        AddDomainEvent(new IssueStatusChangedDomainEvent(Id, oldStatus, newStatus));
    }
}
```
