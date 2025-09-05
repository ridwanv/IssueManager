// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Blazor.Application.Features.Agents.DTOs;

/// <summary>
/// DTO for converting a user to an agent in the UI
/// </summary>
public class ConvertUserToAgentDto
{
    [Range(1, 50, ErrorMessage = "Max concurrent conversations must be between 1 and 50")]
    public int MaxConcurrentConversations { get; set; } = 5;

    [Range(1, 10, ErrorMessage = "Priority must be between 1 and 10")]
    public int Priority { get; set; } = 1;

    [MaxLength(1000, ErrorMessage = "Skills must not exceed 1000 characters")]
    public string? Skills { get; set; }

    [MaxLength(2000, ErrorMessage = "Notes must not exceed 2000 characters")]
    public string? Notes { get; set; }
}
