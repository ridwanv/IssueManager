// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Domain.Enums;

public enum ConversationMode
{
    Bot = 1,
    Escalating = 2,
    Human = 3,
    HandingBackToBot = 4
}