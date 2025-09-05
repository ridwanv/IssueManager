// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace CleanArchitecture.Blazor.Application.Common.Security;

public static partial class Permissions
{
    [DisplayName("Agent Permissions")]
    [Description("Set permissions for agent management operations")]
    public static class Agents
    {
        [Description("Allows viewing agent details")]
        public const string View = "Permissions.Agents.View";

        [Description("Allows creating new agents")]
        public const string Create = "Permissions.Agents.Create";

        [Description("Allows modifying existing agent details")]
        public const string Edit = "Permissions.Agents.Edit";

        [Description("Allows deleting agents")]
        public const string Delete = "Permissions.Agents.Delete";

        [Description("Allows searching for agent records")]
        public const string Search = "Permissions.Agents.Search";

        [Description("Allows importing agent records")]
        public const string Import = "Permissions.Agents.Import";

        [Description("Allows exporting agent records")]
        public const string Export = "Permissions.Agents.Export";

        [Description("Allows managing agent status (Available, Busy, Break, etc.)")]
        public const string ManageStatus = "Permissions.Agents.ManageStatus";

        [Description("Allows managing agent skills and priority")]
        public const string ManageSkills = "Permissions.Agents.ManageSkills";

        [Description("Allows viewing agent conversation assignments")]
        public const string ViewAssignments = "Permissions.Agents.ViewAssignments";

        [Description("Allows managing agent conversation assignments")]
        public const string ManageAssignments = "Permissions.Agents.ManageAssignments";

        [Description("Allows converting existing users to agents")]
        public const string ConvertUser = "Permissions.Agents.ConvertUser";
    }
}

public class AgentsAccessRights
{
    public bool View { get; set; }
    public bool Create { get; set; }
    public bool Edit { get; set; }
    public bool Delete { get; set; }
    public bool Search { get; set; }
    public bool Import { get; set; }
    public bool Export { get; set; }
    public bool ManageStatus { get; set; }
    public bool ManageSkills { get; set; }
    public bool ViewAssignments { get; set; }
    public bool ManageAssignments { get; set; }
    public bool ConvertUser { get; set; }
}
