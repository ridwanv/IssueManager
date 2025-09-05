// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Application.Common.Interfaces.Identity;
using CleanArchitecture.Blazor.Application.Common.Models;
using CleanArchitecture.Blazor.Application.Features.Agents.Caching;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Application.Features.Agents.Commands.UpdateStatus;

public record UpdateAgentStatusCommand(
    int AgentId,
    AgentStatus Status
) : ICacheInvalidatorRequest<Result<bool>>
{
    public string CacheKey => AgentCacheKey.GetAllCacheKey;
    public IEnumerable<string>? Tags => AgentCacheKey.Tags.Concat(new[] { AgentCacheKey.GetCurrentAgentKey });
}

public class UpdateAgentStatusCommandHandler : IRequestHandler<UpdateAgentStatusCommand, Result<bool>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IUserContextAccessor _userContextAccessor;
    private readonly ILogger<UpdateAgentStatusCommandHandler> _logger;

    public UpdateAgentStatusCommandHandler(
        IApplicationDbContextFactory dbContextFactory,
        IUserContextAccessor userContextAccessor,
        ILogger<UpdateAgentStatusCommandHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _userContextAccessor = userContextAccessor;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateAgentStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

            var agent = await db.Agents
                .Include(x => x.ApplicationUser)
                .FirstOrDefaultAsync(x => x.Id == request.AgentId, cancellationToken);

            if (agent == null)
            {
                return await Result<bool>.FailureAsync($"Agent with ID {request.AgentId} not found");
            }

            var oldStatus = agent.Status;
            agent.Status = request.Status;
            agent.LastActiveAt = DateTime.UtcNow;
            agent.LastModified = DateTime.UtcNow;
            agent.LastModifiedBy = _userContextAccessor.Current?.UserId;

            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Agent {AgentId} ({UserName}) status updated from {OldStatus} to {NewStatus}", 
                agent.Id, agent.ApplicationUser.UserName, oldStatus, request.Status);

            return await Result<bool>.SuccessAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating agent {AgentId} status to {Status}", request.AgentId, request.Status);
            return await Result<bool>.FailureAsync("Failed to update agent status");
        }
    }
}

public class UpdateAgentStatusCommandValidator : AbstractValidator<UpdateAgentStatusCommand>
{
    public UpdateAgentStatusCommandValidator()
    {
        RuleFor(x => x.AgentId)
            .GreaterThan(0)
            .WithMessage("Agent ID must be greater than 0");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status must be a valid agent status");
    }
}
