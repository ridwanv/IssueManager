#nullable enable
using System.ComponentModel;

namespace IssueManager.Bot.Models;

/// <summary>
/// Command for creating a new issue via API
/// </summary>
public class CreateIssueCommand
{
    [Description("Title")]
    public string Title { get; set; } = default!;
    
    [Description("Description")]
    public string Description { get; set; } = default!;
    
    [Description("Category")]
    public IssueCategory Category { get; set; }
    
    [Description("Priority")]
    public IssuePriority Priority { get; set; }
    
    [Description("Status")]
    public IssueStatus Status { get; set; } = IssueStatus.New;
    
    [Description("Reporter Contact Id")]
    public int? ReporterContactId { get; set; }
    
    [Description("Conversation Id")]
    public int? ConversationId { get; set; }
    
    [Description("Channel")]
    public string? Channel { get; set; }
    
    [Description("Product")]
    public string? Product { get; set; }
    
    [Description("Severity")]
    public string? Severity { get; set; }
    
    [Description("Summary")]
    public string? Summary { get; set; }
    
    [Description("Consent Flag")]
    public bool ConsentFlag { get; set; } = true;
}

/// <summary>
/// Issue categories
/// </summary>
public enum IssueCategory
{
    Technical = 1,
    Billing = 2,
    General = 3,
    Feature = 4
}

/// <summary>
/// Issue priorities
/// </summary>
public enum IssuePriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// Issue statuses
/// </summary>
public enum IssueStatus
{
    New = 1,
    InProgress = 2,
    Resolved = 3,
    Closed = 4,
    OnHold = 5
}

/// <summary>
/// Result wrapper for API responses
/// </summary>
public class Result<T>
{
    public bool Succeeded { get; set; }
    public T Data { get; set; } = default!;
    public string ErrorMessage { get; set; } = string.Empty;
    public List<string> ErrorMessages { get; set; } = new();
    public int ErrorCode { get; set; }

    public static Task<Result<T>> SuccessAsync(T data, string message = "")
    {
        return Task.FromResult(new Result<T>
        {
            Succeeded = true,
            Data = data,
            ErrorMessage = message
        });
    }

    public static Task<Result<T>> FailureAsync(string errorMessage, int errorCode = 400)
    {
        return Task.FromResult(new Result<T>
        {
            Succeeded = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        });
    }
}

/// <summary>
/// Non-generic result wrapper for API responses
/// </summary>
public class Result
{
    public bool Succeeded { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public List<string> ErrorMessages { get; set; } = new();
    public int ErrorCode { get; set; }

    public static Task<Result> SuccessAsync(string message = "")
    {
        return Task.FromResult(new Result
        {
            Succeeded = true,
            ErrorMessage = message
        });
    }

    public static Task<Result> FailureAsync(string errorMessage, int errorCode = 400)
    {
        return Task.FromResult(new Result
        {
            Succeeded = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        });
    }

    public static Result Success(string message = "")
    {
        return new Result
        {
            Succeeded = true,
            ErrorMessage = message
        };
    }

    public static Result Failure(string errorMessage, int errorCode = 400)
    {
        return new Result
        {
            Succeeded = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        };
    }
}

/// <summary>
/// Issue details DTO
/// </summary>
public class IssueDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public IssueCategory Category { get; set; }
    public IssuePriority Priority { get; set; }
    public IssueStatus Status { get; set; }
    public int? ReporterContactId { get; set; }
    public int? ConversationId { get; set; }
    public string? ReporterName { get; set; }
    public string? ReporterPhone { get; set; }
    public string? Channel { get; set; }
    public string? Product { get; set; }
    public string? Severity { get; set; }
    public string? Summary { get; set; }
    public string? SourceMessageIds { get; set; }
    public string? WhatsAppMetadata { get; set; }
    public bool ConsentFlag { get; set; }
    public Guid? DuplicateOfId { get; set; }
    public string TenantId { get; set; } = default!;
    public DateTime? Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}

/// <summary>
/// Issue list DTO for pagination
/// </summary>
public class IssueListDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public IssueCategory Category { get; set; }
    public IssuePriority Priority { get; set; }
    public IssueStatus Status { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? LastModified { get; set; }
    public int? ReporterContactId { get; set; }
    public string? ReporterName { get; set; }
    public string? ReporterPhone { get; set; }
    public string? AssignedUserName { get; set; }
    public string? Channel { get; set; }
    public string? Product { get; set; }
}

/// <summary>
/// Paginated data wrapper
/// </summary>
public class PaginatedData<T>
{
    public PaginatedData(IEnumerable<T> items, int total, int pageIndex, int pageSize)
    {
        Items = items;
        TotalItems = total;
        CurrentPage = pageIndex;
        TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        HasPreviousPage = pageIndex > 1;
        HasNextPage = pageIndex < TotalPages;
    }

    public int CurrentPage { get; }
    public int TotalItems { get; set; }
    public int TotalPages { get; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    public IEnumerable<T> Items { get; set; }

    public static Task<PaginatedData<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
    {
        var total = source.Count();
        var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult(new PaginatedData<T>(items, total, pageIndex, pageSize));
    }
}

/// <summary>
/// Conversation message DTO for API responses
/// </summary>
public class ConversationMessageDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public string BotFrameworkConversationId { get; set; } = default!;
    public string Role { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string? ToolCallId { get; set; }
    public string? ToolCalls { get; set; }
    public string? ImageType { get; set; }
    public string? ImageData { get; set; }
    public string? Attachments { get; set; }
    public DateTime Timestamp { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? ChannelId { get; set; }
    public bool IsEscalated { get; set; }
    public string TenantId { get; set; } = default!;
}

/// <summary>
/// Create conversation message DTO for API requests
/// </summary>
public class ConversationMessageCreateDto
{
    public string BotFrameworkConversationId { get; set; } = default!;
    public string Role { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string? ToolCallId { get; set; }
    public string? ToolCalls { get; set; }
    public string? ImageType { get; set; }
    public string? ImageData { get; set; }
    public string? Attachments { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? ChannelId { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? ConversationChannelData { get; set; } // Full ConversationReference JSON for Bot Framework routing
}

/// <summary>
/// Urgency levels for agent notifications
/// </summary>
public enum NotificationUrgency
{
    Low,
    Normal,
    High,
    Critical
}