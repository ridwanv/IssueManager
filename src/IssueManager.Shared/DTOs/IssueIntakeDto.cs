using System.ComponentModel.DataAnnotations;

namespace IssueManager.Shared.DTOs;

/// <summary>
/// DTO for creating a new issue via intake process (WhatsApp/Bot)
/// </summary>
public record IssueIntakeDto
{
    /// <summary>
    /// Reporter's phone number
    /// </summary>
    [Required]
    public required string ReporterPhone { get; set; }

    /// <summary>
    /// Reporter's name (optional)
    /// </summary>
    public string? ReporterName { get; set; }

    /// <summary>
    /// Channel through which the issue was reported (e.g., WhatsApp, Bot)
    /// </summary>
    [Required]
    public required string Channel { get; set; }

    /// <summary>
    /// Issue category
    /// </summary>
    [Required]
    public required string Category { get; set; }

    /// <summary>
    /// Product related to the issue
    /// </summary>
    [Required]
    public required string Product { get; set; }

    /// <summary>
    /// Severity level of the issue
    /// </summary>
    [Required]
    public required string Severity { get; set; }

    /// <summary>
    /// Priority level of the issue
    /// </summary>
    [Required]
    public required string Priority { get; set; }

    /// <summary>
    /// Brief summary of the issue
    /// </summary>
    [Required]
    public required string Summary { get; set; }

    /// <summary>
    /// Detailed description of the issue
    /// </summary>
    [Required]
    public required string Description { get; set; }

    /// <summary>
    /// Source message IDs for tracking
    /// </summary>
    public string? SourceMessageIds { get; set; }

    /// <summary>
    /// Consent flag for data processing
    /// </summary>
    public bool ConsentFlag { get; set; } = true;

    /// <summary>
    /// Initial status of the issue
    /// </summary>
    public string Status { get; set; } = "New";

    /// <summary>
    /// Associated contact ID if available
    /// </summary>
    public Guid? ContactId { get; set; }



    public string? ConversationReference { get; set; } 

    /// <summary>
    /// Attachments related to the issue
    /// </summary>
    public List<IssueAttachmentDto>? Attachments { get; set; }
}

/// <summary>
/// DTO for issue attachments
/// </summary>
public record IssueAttachmentDto
{
    /// <summary>
    /// File name
    /// </summary>
    [Required]
    public required string Name { get; set; }

    /// <summary>
    /// MIME content type
    /// </summary>
    [Required]
    public required string ContentType { get; set; }

    /// <summary>
    /// URL to the attachment
    /// </summary>
    [Required]
    public required string Url { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long Size { get; set; }
}