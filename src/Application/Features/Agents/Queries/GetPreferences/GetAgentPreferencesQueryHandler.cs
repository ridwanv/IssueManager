// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using AutoMapper;
using CleanArchitecture.Blazor.Application.Features.Agents.DTOs;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;

namespace CleanArchitecture.Blazor.Application.Features.Agents.Queries.GetPreferences;

public class GetAgentPreferencesQueryHandler : IRequestHandler<GetAgentPreferencesQuery, Result<AgentNotificationPreferencesDto?>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;
    private readonly IUserContextAccessor _userContextAccessor;

    public GetAgentPreferencesQueryHandler(
        IApplicationDbContextFactory dbContextFactory,
        IMapper mapper,
        IUserContextAccessor userContextAccessor)
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
        _userContextAccessor = userContextAccessor;
    }

    public async Task<Result<AgentNotificationPreferencesDto?>> Handle(GetAgentPreferencesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
            
            var currentUser = _userContextAccessor.Current;
            if (currentUser == null || string.IsNullOrEmpty(currentUser.TenantId))
            {
                return Result<AgentNotificationPreferencesDto?>.Failure("User context not available.");
            }
            
            var preferences = await db.AgentNotificationPreferences
                .FirstOrDefaultAsync(x => x.ApplicationUserId == request.ApplicationUserId && 
                                         x.TenantId == currentUser.TenantId, 
                                    cancellationToken);
            
            if (preferences == null)
            {
                // Return default preferences if none exist
                return Result<AgentNotificationPreferencesDto?>.Success(new AgentNotificationPreferencesDto
                {
                    ApplicationUserId = request.ApplicationUserId,
                    TenantId = currentUser.TenantId,
                    EnableBrowserNotifications = true,
                    EnableAudioAlerts = true,
                    EnableEmailNotifications = false,
                    NotifyOnStandardPriority = true,
                    NotifyOnHighPriority = true,
                    NotifyOnCriticalPriority = true,
                    NotifyDuringBreak = false,
                    NotifyWhenOffline = false,
                    AudioVolume = 50
                });
            }
            
            var dto = _mapper.Map<AgentNotificationPreferencesDto>(preferences);
            return Result<AgentNotificationPreferencesDto?>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<AgentNotificationPreferencesDto?>.Failure($"Failed to get agent preferences: {ex.Message}");
        }
    }
}