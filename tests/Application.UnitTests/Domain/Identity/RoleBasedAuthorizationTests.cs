// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.Constants.Roles;
using CleanArchitecture.Blazor.Domain.Enums;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;

namespace IssueManager.Application.UnitTests.Domain.Identity;

[TestFixture]
public class RoleBasedAuthorizationTests
{
    [Test]
    public void RoleName_ShouldHaveAllPersonaRoles()
    {
        // Arrange & Act
        var personaRoles = RoleName.PersonaRoles;

        // Assert
        personaRoles.Should().Contain(RoleName.PlatformOwner);
        personaRoles.Should().Contain(RoleName.TenantOwner);
        personaRoles.Should().Contain(RoleName.IssueManager);
        personaRoles.Should().Contain(RoleName.IssueAssignee);
        personaRoles.Should().Contain(RoleName.ChatAgent);
        personaRoles.Should().Contain(RoleName.ChatSupervisor);
        personaRoles.Should().Contain(RoleName.EndUser);
        personaRoles.Should().Contain(RoleName.ApiConsumer);
        personaRoles.Should().HaveCount(8);
    }

    [Test]
    public void RoleName_ShouldHaveCorrectMappingToUserType()
    {
        // Assert mapping consistency between UserType enum and role names
        RoleName.PlatformOwner.Should().Be(UserType.PlatformOwner.ToString());
        RoleName.TenantOwner.Should().Be(UserType.TenantOwner.ToString());
        RoleName.IssueManager.Should().Be(UserType.IssueManager.ToString());
        RoleName.IssueAssignee.Should().Be(UserType.IssueAssignee.ToString());
        RoleName.ChatAgent.Should().Be(UserType.ChatAgent.ToString());
        RoleName.ChatSupervisor.Should().Be(UserType.ChatSupervisor.ToString());
        RoleName.EndUser.Should().Be(UserType.EndUser.ToString());
        RoleName.ApiConsumer.Should().Be(UserType.ApiConsumer.ToString());
    }

    [Test]
    public void RoleName_AllRoles_ShouldIncludeLegacyAndPersonaRoles()
    {
        // Arrange & Act
        var allRoles = RoleName.AllRoles;

        // Assert - should contain all legacy roles
        allRoles.Should().Contain(RoleName.Admin);
        allRoles.Should().Contain(RoleName.Basic);
        allRoles.Should().Contain(RoleName.Users);
        
        // Assert - should contain all persona roles
        allRoles.Should().Contain(RoleName.PlatformOwner);
        allRoles.Should().Contain(RoleName.TenantOwner);
        allRoles.Should().Contain(RoleName.IssueManager);
        allRoles.Should().Contain(RoleName.IssueAssignee);
        allRoles.Should().Contain(RoleName.ChatAgent);
        allRoles.Should().Contain(RoleName.ChatSupervisor);
        allRoles.Should().Contain(RoleName.EndUser);
        allRoles.Should().Contain(RoleName.ApiConsumer);
        
        allRoles.Should().HaveCount(11); // 3 legacy + 8 persona roles
    }

    [Test]
    public void RoleName_PersonaRoles_ShouldNotIncludeLegacyRoles()
    {
        // Arrange & Act
        var personaRoles = RoleName.PersonaRoles;

        // Assert - should NOT contain legacy roles
        personaRoles.Should().NotContain(RoleName.Admin);
        personaRoles.Should().NotContain(RoleName.Basic);
        personaRoles.Should().NotContain(RoleName.Users);
    }

    [Test]
    public void RoleConstants_ShouldBeStringConstants()
    {
        // Assert that all role constants are string values matching their names
        typeof(string).Should().Be(RoleName.PlatformOwner.GetType());
        typeof(string).Should().Be(RoleName.TenantOwner.GetType());
        typeof(string).Should().Be(RoleName.IssueManager.GetType());
        typeof(string).Should().Be(RoleName.IssueAssignee.GetType());
        typeof(string).Should().Be(RoleName.ChatAgent.GetType());
        typeof(string).Should().Be(RoleName.ChatSupervisor.GetType());
        typeof(string).Should().Be(RoleName.EndUser.GetType());
        typeof(string).Should().Be(RoleName.ApiConsumer.GetType());
    }

    [Test]
    public void PersonaRoles_ShouldHaveUniqueNames()
    {
        // Arrange & Act
        var personaRoles = RoleName.PersonaRoles;
        var distinctRoles = personaRoles.Distinct().ToArray();

        // Assert
        distinctRoles.Should().HaveCount(personaRoles.Length);
    }

    [Test]
    public void AllRoles_ShouldHaveUniqueNames()
    {
        // Arrange & Act
        var allRoles = RoleName.AllRoles;
        var distinctRoles = allRoles.Distinct().ToArray();

        // Assert
        distinctRoles.Should().HaveCount(allRoles.Length);
    }

    [Test]
    public void RoleName_ShouldFollowNamingConvention()
    {
        // Assert that all persona role names follow PascalCase convention
        var personaRoles = RoleName.PersonaRoles;
        
        foreach (var role in personaRoles)
        {
            role.Should().MatchRegex(@"^[A-Z][a-zA-Z]*$", 
                $"Role '{role}' should follow PascalCase naming convention");
            role.Should().NotContain(" ", $"Role '{role}' should not contain spaces");
            role.Should().NotBeNullOrEmpty($"Role '{role}' should not be null or empty");
        }
    }
}