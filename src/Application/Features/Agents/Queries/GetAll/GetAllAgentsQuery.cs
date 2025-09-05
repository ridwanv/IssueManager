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

namespace CleanArchitecture.Blazor.Application.Features.Agents.Queries.GetAll;

public record GetAllAgentsQuery : ICacheableRequest<Result<List<AgentDto>>>
{
    public string CacheKey => AgentCacheKey.GetAllCacheKey;
    public IEnumerable<string>? Tags => AgentCacheKey.Tags;
    public TimeSpan? Expiry => TimeSpan.FromMinutes(30);
}

public class GetAllAgentsQueryHandler : IRequestHandler<GetAllAgentsQuery, Result<List<AgentDto>>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;

    public GetAllAgentsQueryHandler(
        IApplicationDbContextFactory dbContextFactory,
        IMapper mapper)
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
    }

    public async Task<Result<List<AgentDto>>> Handle(GetAllAgentsQuery request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
        
        var agents = await db.Agents
            .Include(x => x.ApplicationUser)
            .ThenInclude(u => u.Tenant)
            .AsNoTracking()
            .ProjectTo<AgentDto>(_mapper.ConfigurationProvider)
            .OrderBy(x => x.User!.UserName)
            .ToListAsync(cancellationToken);

        return Result<List<AgentDto>>.Success(agents);
    }
}
