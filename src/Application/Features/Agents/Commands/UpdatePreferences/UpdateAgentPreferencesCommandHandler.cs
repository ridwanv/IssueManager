// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using AutoMapper;
using CleanArchitecture.Blazor.Application.Features.Agents.DTOs;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;

namespace CleanArchitecture.Blazor.Application.Features.Agents.Commands.UpdatePreferences;

public class UpdateAgentPreferencesCommandHandler : IRequestHandler<UpdateAgentPreferencesCommand, Result<AgentNotificationPreferencesDto>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;
    private readonly IUserContextAccessor _userContextAccessor;

    public UpdateAgentPreferencesCommandHandler(
        IApplicationDbContextFactory dbContextFactory,
        IMapper mapper,
        IUserContextAccessor userContextAccessor)
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
        _userContextAccessor = userContextAccessor;
    }

    public async Task<Result<AgentNotificationPreferencesDto>> Handle(UpdateAgentPreferencesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
            
            var currentUser = _userContextAccessor.Current;
            if (currentUser == null || string.IsNullOrEmpty(currentUser.TenantId))
            {
                return Result<AgentNotificationPreferencesDto>.Failure("User context not available.");
            }
            
            var preferences = await db.AgentNotificationPreferences
                .FirstOrDefaultAsync(x => x.ApplicationUserId == request.ApplicationUserId && 
                                         x.TenantId == currentUser.TenantId, 
                                    cancellationToken);
            
            if (preferences == null)
            {
                preferences = new AgentNotificationPreferences
                {
                    ApplicationUserId = request.ApplicationUserId,
                    TenantId = currentUser.TenantId
                };
                db.AgentNotificationPreferences.Add(preferences);
            }
            
            preferences.EnableBrowserNotifications = request.EnableBrowserNotifications;
            preferences.EnableAudioAlerts = request.EnableAudioAlerts;
            preferences.EnableEmailNotifications = request.EnableEmailNotifications;
            preferences.NotifyOnStandardPriority = request.NotifyOnStandardPriority;
            preferences.NotifyOnHighPriority = request.NotifyOnHighPriority;
            preferences.NotifyOnCriticalPriority = request.NotifyOnCriticalPriority;
            preferences.NotifyDuringBreak = request.NotifyDuringBreak;
            preferences.NotifyWhenOffline = request.NotifyWhenOffline;
            preferences.AudioVolume = Math.Max(0, Math.Min(100, request.AudioVolume));
            preferences.CustomSoundUrl = request.CustomSoundUrl;
            
            await db.SaveChangesAsync(cancellationToken);
            
            var dto = _mapper.Map<AgentNotificationPreferencesDto>(preferences);
            return Result<AgentNotificationPreferencesDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<AgentNotificationPreferencesDto>.Failure($"Failed to update agent preferences: {ex.Message}");
        }
    }
}