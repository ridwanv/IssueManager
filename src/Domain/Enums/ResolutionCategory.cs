// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Domain.Enums;

public enum ResolutionCategory
{
    Resolved = 1,
    EscalatedToTechnicalTeam = 2,
    InformationProvided = 3,
    CustomerNoResponse = 4,
    DuplicateIssue = 5,
    CannotReproduce = 6
}