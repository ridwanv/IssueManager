using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Features.Issues.Caching;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Events;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Application.Features.Issues.Commands.Create;

public class IssueIntakeCommandHandler : IRequestHandler<IssueIntakeCommand, Result<Guid>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;
    private readonly IIssueReferenceNumberService _referenceNumberService;

    public IssueIntakeCommandHandler(
        IApplicationDbContextFactory dbContextFactory,
        IMapper mapper,
        IIssueReferenceNumberService referenceNumberService
    )
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
        _referenceNumberService = referenceNumberService;
    }

    public async Task<Result<Guid>> Handle(IssueIntakeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
            
            // Find or create contact
            var contact = await FindOrCreateContactAsync(db, request.ReporterPhone, request.ReporterName, cancellationToken);
            
            // Generate unique reference number with retry logic
            var referenceNumber = await GenerateUniqueReferenceNumberAsync(cancellationToken);
            
            // Parse enum values from string inputs
            if (!Enum.TryParse<IssueCategory>(request.Category, true, out var category))
            {
                category = IssueCategory.General; // Default fallback
            }
            
            if (!Enum.TryParse<IssuePriority>(request.Priority, true, out var priority))
            {
                priority = IssuePriority.Medium; // Default fallback
            }
            
            // Create WhatsApp metadata
            var whatsAppMetadata = System.Text.Json.JsonSerializer.Serialize(new
            {
                Channel = request.Channel,
                Product = request.Product,
                Severity = request.Severity,
                ConversationState = "Intake Complete",
                ProcessedAt = DateTime.UtcNow
            });
            
            // Create the issue using factory method
            var issue = Issue.Create(
                referenceNumber: referenceNumber,
                title: request.Summary,
                description: request.Description,
                category: category,
                priority: priority,
                reporterContactId: contact.Id,
                tenantId: contact.TenantId,
                sourceMessageIds: request.SourceMessageIds,
                whatsAppMetadata: whatsAppMetadata,
                consentFlag: request.ConsentFlag,
                conversationId: request.ConversationId
            );
            
            // Set legacy fields for backward compatibility
            issue.ReporterPhone = request.ReporterPhone;
            issue.ReporterName = request.ReporterName;
            issue.Summary = request.Summary;
            issue.Product = request.Product;
            issue.Severity = request.Severity;
            
            // Add attachments if provided
            if (request.Attachments != null && request.Attachments.Any())
            {
                foreach (var attachmentData in request.Attachments)
                {
                    var attachment = new Attachment
                    {
                        Id = Guid.NewGuid(),
                        IssueId = issue.Id,
                        Uri = attachmentData.Url,
                        Type = attachmentData.ContentType,
                        SizeBytes = attachmentData.Size,
                        CreatedUtc = DateTime.UtcNow,
                        ScanStatus = "Pending",
                        TenantId = contact.TenantId
                    };
                    issue.Attachments.Add(attachment);
                }
            }
            
            // Save with retry logic for transient errors
            await SaveWithRetryAsync(db, issue, cancellationToken);
            
            return await Result<Guid>.SuccessAsync(issue.Id);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("duplicate") || ex.Message.Contains("unique"))
        {
            return await Result<Guid>.FailureAsync("Unable to generate unique issue reference. Please try again.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return await Result<Guid>.FailureAsync("Issue creation failed due to a concurrent update. Please try again.");
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("FOREIGN KEY") == true)
        {
            return await Result<Guid>.FailureAsync("Issue creation failed due to invalid contact reference.");
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return await Result<Guid>.FailureAsync("Issue creation was cancelled due to timeout.");
        }
        catch (Exception)
        {
            // Log the full exception details for debugging (would use ILogger in real implementation)
            return await Result<Guid>.FailureAsync($"Issue creation failed due to an unexpected error. Please contact support if this persists. Reference: {Guid.NewGuid()}");
        }
    }

    private async Task<string> GenerateUniqueReferenceNumberAsync(CancellationToken cancellationToken, int maxRetries = 3)
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var referenceNumber = await _referenceNumberService.GenerateReferenceNumberAsync(cancellationToken);
                
                // Verify uniqueness before returning
                await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
                var exists = await db.Issues.AnyAsync(i => i.ReferenceNumber == referenceNumber, cancellationToken);
                
                if (!exists)
                {
                    return referenceNumber;
                }
                
                if (attempt == maxRetries - 1)
                {
                    throw new InvalidOperationException("Unable to generate unique reference number after multiple attempts");
                }
                
                // Brief delay before retry
                await Task.Delay(100 * (attempt + 1), cancellationToken);
            }
            catch (Exception) when (attempt < maxRetries - 1)
            {
                // Log retry attempt (would use ILogger in real implementation)
                await Task.Delay(200 * (attempt + 1), cancellationToken);
            }
        }
        
        throw new InvalidOperationException("Failed to generate unique reference number");
    }

    private async Task SaveWithRetryAsync(IApplicationDbContext db, Issue issue, CancellationToken cancellationToken, int maxRetries = 2)
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                db.Issues.Add(issue);
                await db.SaveChangesAsync(cancellationToken);
                return;
            }
            catch (DbUpdateConcurrencyException) when (attempt < maxRetries - 1)
            {
                // Remove from context and retry
                db.Issues.Remove(issue);
                await Task.Delay(100 * (attempt + 1), cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("timeout") == true && attempt < maxRetries - 1)
            {
                // Database timeout - retry
                db.Issues.Remove(issue);
                await Task.Delay(500 * (attempt + 1), cancellationToken);
            }
        }
        
        // Final attempt without retry
        db.Issues.Add(issue);
        await db.SaveChangesAsync(cancellationToken);
    }
    
    private async Task<Contact> FindOrCreateContactAsync(IApplicationDbContext db, string phone, string? name, CancellationToken cancellationToken)
    {
        // Try to find existing contact by phone number (with tenant isolation)
        var existingContact = await db.Contacts
            .FirstOrDefaultAsync(c => c.PhoneNumber == phone, cancellationToken);
            
        if (existingContact != null)
        {
            // Update name if provided and different
            if (!string.IsNullOrWhiteSpace(name) && existingContact.Name != name)
            {
                existingContact.Name = name;
                existingContact.LastModified = DateTime.UtcNow;
            }
            return existingContact;
        }
        
        // Create new contact
        var newContact = new Contact
        {
            PhoneNumber = phone,
            Name = name,
            Description = "Auto-created from WhatsApp issue intake",
            // TenantId will be set by the system based on current context
        };
        
        db.Contacts.Add(newContact);
        return newContact;
    }
}