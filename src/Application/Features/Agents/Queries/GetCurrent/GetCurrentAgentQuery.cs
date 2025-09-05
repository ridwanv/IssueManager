// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;
using CleanArchitecture.Blazor.Application.Common.Models;
using CleanArchitecture.Blazor.Application.Features.Agents.DTOs;
using CleanArchitecture.Blazor.Application.Features.Agents.Caching;

namespace CleanArchitecture.Blazor.Application.Features.Agents.Queries.GetCurrent;

public record GetCurrentAgentQuery : ICacheableRequest<Result<AgentDto?>>
{
    public string CacheKey => AgentCacheKey.GetCurrentAgentKey;
    public IEnumerable<string>? Tags => AgentCacheKey.Tags;
    public TimeSpan? Expiry => TimeSpan.FromMinutes(5); // Shorter expiry for current user data
}

public class GetCurrentAgentQueryHandler : IRequestHandler<GetCurrentAgentQuery, Result<AgentDto?>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IUserContextAccessor _userContextAccessor;
    private readonly IMapper _mapper;

    public GetCurrentAgentQueryHandler(
        IApplicationDbContextFactory dbContextFactory,
        IUserContextAccessor userContextAccessor,
        IMapper mapper)
    {
        _dbContextFactory = dbContextFactory;
        _userContextAccessor = userContextAccessor;
        _mapper = mapper;
    }

    public async Task<Result<AgentDto?>> Handle(GetCurrentAgentQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _userContextAccessor.Current?.UserId;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Result<AgentDto?>.Success(null);
        }

        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
        
        var agent = await db.Agents
            .Include(x => x.ApplicationUser)
            .ThenInclude(u => u.Tenant)
            .AsNoTracking()
            .Where(x => x.ApplicationUser.Id == currentUserId)
            .ProjectTo<AgentDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return Result<AgentDto?>.Success(agent);
    }
}