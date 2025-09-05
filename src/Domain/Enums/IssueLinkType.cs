// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Domain.Enums
{
    public enum IssueLinkType
    {
        /// <summary>
        /// Child issue is a duplicate of the parent issue
        /// </summary>
        Duplicate = 0,
        
        /// <summary>
        /// Issues are related but not duplicates
        /// </summary>
        Related = 1,
        
        /// <summary>
        /// Parent issue blocks the child issue from being resolved
        /// </summary>
        Blocks = 2,
        
        /// <summary>
        /// Child issue is caused by the parent issue
        /// </summary>
        CausedBy = 3,
        
        /// <summary>
        /// Issues are part of the same incident or broader problem
        /// </summary>
        PartOf = 4
    }
}