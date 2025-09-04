using CleanArchitecture.Blazor.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Blazor.Application.Features.Issues.Commands.Create;

public interface IIssueReferenceNumberService
{
    Task<string> GenerateReferenceNumberAsync(CancellationToken cancellationToken = default);
}

public class IssueReferenceNumberService : IIssueReferenceNumberService
{
    private readonly IApplicationDbContextFactory _dbContextFactory;

    public IssueReferenceNumberService(IApplicationDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<string> GenerateReferenceNumberAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
        
        var currentYear = DateTime.UtcNow.Year;
        var prefix = $"ISS-{currentYear}-";
        
        // Find the highest sequence number for the current year (optimized query)
        var referenceNumbers = await db.Issues
            .Where(i => i.ReferenceNumber.StartsWith(prefix) && i.ReferenceNumber.Length >= 13)
            .Select(i => i.ReferenceNumber.Substring(9)) // Extract sequence part "NNNNNN"
            .ToListAsync(cancellationToken);
        
        var highestSequence = referenceNumbers
            .Where(seq => int.TryParse(seq, out _)) // Filter valid numeric sequences
            .Select(seq => int.Parse(seq))
            .DefaultIfEmpty(0)
            .Max();

        var nextSequence = highestSequence + 1;
        return $"ISS-{currentYear}-{nextSequence:D6}";
    }
}
