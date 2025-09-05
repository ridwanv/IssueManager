// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using AutoMapper;
using CleanArchitecture.Blazor.Application.Features.Identity.DTOs;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Application.Features.Agents.DTOs;

[Description("Agent")]
public class AgentDto
{
    [Description("Id")]
    public int Id { get; set; }

    [Description("Application User Id")]
    public string ApplicationUserId { get; set; } = default!;

    [Description("User")]
    public ApplicationUserDto? User { get; set; }

    [Description("Status")]
    public AgentStatus Status { get; set; } = AgentStatus.Offline;

    [Description("Max Concurrent Conversations")]
    public int MaxConcurrentConversations { get; set; } = 5;

    [Description("Active Conversation Count")]
    public int ActiveConversationCount { get; set; } = 0;

    [Description("Last Active At")]
    public DateTime? LastActiveAt { get; set; }

    [Description("Skills")]
    public string? Skills { get; set; }

    [Description("Priority")]
    public int Priority { get; set; } = 1;

    [Description("Notes")]
    public string? Notes { get; set; }

    [Description("Tenant Id")]
    public string TenantId { get; set; } = default!;

    [Description("Created")]
    public DateTime? Created { get; set; }

    [Description("Created By")]
    public string? CreatedBy { get; set; }

    [Description("Last Modified")]
    public DateTime? LastModified { get; set; }

    [Description("Last Modified By")]
    public string? LastModifiedBy { get; set; }

    // Computed properties
    public bool IsAvailable => Status == AgentStatus.Available && ActiveConversationCount < MaxConcurrentConversations;
    public bool CanTakeConversations => IsAvailable && Status != AgentStatus.Break && Status != AgentStatus.Offline;
    public string StatusDisplay => Status.ToString();
    public string SkillsList => !string.IsNullOrEmpty(Skills) ? Skills : "No skills defined";

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Agent, AgentDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.ApplicationUser));
        }
    }
}
