// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Domain.Enums;

public enum HandoffType
{
    BotToHuman = 1,
    HumanToBot = 2,
    AgentToAgent = 3,
    EscalateToSupervisor = 4
}