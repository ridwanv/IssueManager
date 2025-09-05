using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Events;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Blazor.Application.Features.Issues.Commands.UpdateIssue;

public class UpdateIssueCommandHandler : IRequestHandler<UpdateIssueCommand, Result<Guid>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;
    public UpdateIssueCommandHandler(
        IApplicationDbContextFactory dbContextFactory,
        IMapper mapper
    )
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
    }
    public async Task<Result<Guid>> Handle(UpdateIssueCommand request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
        var item = await db.Issues.FindAsync(new object[] { request.Id }, cancellationToken);
        if (item == null) return await Result<Guid>.FailureAsync($"Issue with id: [{request.Id}] not found.");
        _mapper.Map(request, item);
        await db.SaveChangesAsync(cancellationToken);
        return await Result<Guid>.SuccessAsync(item.Id);
    }
}
