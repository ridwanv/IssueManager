// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.BotBuilderSamples
{
    // Defines a state property used to track information about the user.
    public class UserProfile
    {
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime? LastActivity { get; set; }
        public int MessageCount { get; set; }
    }
}
