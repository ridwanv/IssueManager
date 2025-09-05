// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Models;
using CleanArchitecture.Blazor.Application.Features.Agents.DTOs;
using CleanArchitecture.Blazor.Application.Features.Agents.Caching;

namespace CleanArchitecture.Blazor.Application.Features.Agents.Queries.GetById;

public record GetAgentByIdQuery(int Id) : ICacheableRequest<Result<AgentDto>>
{
    public string CacheKey => AgentCacheKey.GetByIdKey(Id);
    public IEnumerable<string>? Tags => AgentCacheKey.Tags;
    public TimeSpan? Expiry => TimeSpan.FromMinutes(30);
}

public class GetAgentByIdQueryHandler : IRequestHandler<GetAgentByIdQuery, Result<AgentDto>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;

    public GetAgentByIdQueryHandler(
        IApplicationDbContextFactory dbContextFactory,
        IMapper mapper)
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
    }

    public async Task<Result<AgentDto>> Handle(GetAgentByIdQuery request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
        
        var agent = await db.Agents
            .Include(x => x.ApplicationUser)
            .ThenInclude(u => u.Tenant)
            .AsNoTracking()
            .ProjectTo<AgentDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (agent == null)
            return Result<AgentDto>.Failure($"Agent with ID {request.Id} not found");

        return Result<AgentDto>.Success(agent);
    }
}
