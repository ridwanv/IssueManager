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
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using CleanArchitecture.Blazor.Domain.Events;

namespace CleanArchitecture.Blazor.Application.Features.Agents.Commands.ConvertUser;

public record ConvertUserToAgentCommand(
    string UserId,
    int MaxConcurrentConversations = 5,
    int Priority = 1,
    string? Skills = null,
    string? Notes = null
) : ICacheInvalidatorRequest<Result<int>>
{
    public string CacheKey => AgentCacheKey.GetAllCacheKey;
    public IEnumerable<string>? Tags => AgentCacheKey.Tags;
}

public class ConvertUserToAgentCommandHandler : IRequestHandler<ConvertUserToAgentCommand, Result<int>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;
    private readonly IUserContextAccessor _userContextAccessor;
    private readonly ILogger<ConvertUserToAgentCommandHandler> _logger;

    public ConvertUserToAgentCommandHandler(
        IApplicationDbContextFactory dbContextFactory,
        IMapper mapper,
        IUserContextAccessor userContextAccessor,
        ILogger<ConvertUserToAgentCommandHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
        _userContextAccessor = userContextAccessor;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(ConvertUserToAgentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await using var db = await _dbContextFactory.CreateAsync(cancellationToken);

            // Check if user exists
            var user = await db.Users
                .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                return await Result<int>.FailureAsync($"User with ID {request.UserId} not found.");
            }

            // Check if user is already an agent
            var existingAgent = await db.Agents
                .FirstOrDefaultAsync(x => x.ApplicationUserId == request.UserId, cancellationToken);

            if (existingAgent != null)
            {
                return await Result<int>.FailureAsync($"User {user.UserName} is already an agent.");
            }

            // Create new agent entity
            var agent = new Agent
            {
                ApplicationUserId = request.UserId,
                Status = AgentStatus.Offline,
                MaxConcurrentConversations = request.MaxConcurrentConversations,
                ActiveConversationCount = 0,
                LastActiveAt = DateTime.UtcNow,
                Skills = request.Skills,
                Priority = request.Priority,
                Notes = request.Notes,
                TenantId = user.TenantId!,
                Created = DateTime.UtcNow,
                CreatedBy = _userContextAccessor.Current?.UserId
            };

            // Add domain event
            agent.AddDomainEvent(new AgentCreatedEvent(agent));

            db.Agents.Add(agent);
            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {UserName} (ID: {UserId}) converted to agent with ID: {AgentId}", 
                user.UserName, user.Id, agent.Id);

            return await Result<int>.SuccessAsync(agent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting user {UserId} to agent", request.UserId);
            return await Result<int>.FailureAsync("Failed to convert user to agent");
        }
    }
}

public class ConvertUserToAgentCommandValidator : AbstractValidator<ConvertUserToAgentCommand>
{
    public ConvertUserToAgentCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.MaxConcurrentConversations)
            .GreaterThan(0)
            .LessThanOrEqualTo(50)
            .WithMessage("Max concurrent conversations must be between 1 and 50");

        RuleFor(x => x.Priority)
            .GreaterThan(0)
            .LessThanOrEqualTo(10)
            .WithMessage("Priority must be between 1 and 10");

        RuleFor(x => x.Skills)
            .MaximumLength(1000)
            .WithMessage("Skills must not exceed 1000 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Notes must not exceed 2000 characters");
    }
}
