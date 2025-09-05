// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Infrastructure.Services.Jira.Models;

namespace CleanArchitecture.Blazor.Infrastructure.Services.Jira;

/// <summary>
/// Extended service interface for JIRA integration operations (Infrastructure layer)
/// </summary>
public interface IExtendedJiraService : IJiraServiceExtended
{
    // This interface can be extended with additional methods specific to the Infrastructure layer
    // Currently, it inherits all methods from IJiraServiceExtended which includes Application.IJiraService
}
