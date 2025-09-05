// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Domain.Enums;

public enum HandoffStatus
{
    Initiated = 1,
    Accepted = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}