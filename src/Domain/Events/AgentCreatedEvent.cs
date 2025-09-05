// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Common.Events;
using CleanArchitecture.Blazor.Domain.Entities;

namespace CleanArchitecture.Blazor.Domain.Events;

public class AgentCreatedEvent : DomainEvent
{
    public AgentCreatedEvent(Agent item)
    {
        Item = item;
    }

    public Agent Item { get; }
}
