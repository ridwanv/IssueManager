// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Domain.Enums;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace IssueManager.Application.UnitTests.Domain.Enums;

[TestFixture]
[Obsolete("UserType enum is being deprecated in favor of role-based authorization. These tests are maintained for backward compatibility only.")]
public class UserTypeTests
{
    [Test]
    public void UserType_ShouldHaveAllExpectedValues()
    {
        // Arrange & Act
        var userTypes = Enum.GetValues<UserType>();

        // Assert
        userTypes.Should().Contain(UserType.PlatformOwner);
        userTypes.Should().Contain(UserType.TenantOwner);
        userTypes.Should().Contain(UserType.IssueManager);
        userTypes.Should().Contain(UserType.IssueAssignee);
        userTypes.Should().Contain(UserType.ChatAgent);
        userTypes.Should().Contain(UserType.ChatSupervisor);
        userTypes.Should().Contain(UserType.EndUser);
        userTypes.Should().Contain(UserType.ApiConsumer);
        userTypes.Should().HaveCount(8);
    }

    [Test]
    public void UserType_ShouldHaveCorrectIntegerValues()
    {
        // Assert
        ((int)UserType.PlatformOwner).Should().Be(1);
        ((int)UserType.TenantOwner).Should().Be(2);
        ((int)UserType.IssueManager).Should().Be(3);
        ((int)UserType.IssueAssignee).Should().Be(4);
        ((int)UserType.ChatAgent).Should().Be(5);
        ((int)UserType.ChatSupervisor).Should().Be(6);
        ((int)UserType.EndUser).Should().Be(7);
        ((int)UserType.ApiConsumer).Should().Be(8);
    }

    [Test]
    public void UserType_ToString_ShouldReturnCorrectNames()
    {
        // Assert
        UserType.PlatformOwner.ToString().Should().Be("PlatformOwner");
        UserType.TenantOwner.ToString().Should().Be("TenantOwner");
        UserType.IssueManager.ToString().Should().Be("IssueManager");
        UserType.IssueAssignee.ToString().Should().Be("IssueAssignee");
        UserType.ChatAgent.ToString().Should().Be("ChatAgent");
        UserType.ChatSupervisor.ToString().Should().Be("ChatSupervisor");
        UserType.EndUser.ToString().Should().Be("EndUser");
        UserType.ApiConsumer.ToString().Should().Be("ApiConsumer");
    }

    [Test]
    public void UserType_CanBeCastFromInt()
    {
        // Arrange & Act & Assert
        ((UserType)1).Should().Be(UserType.PlatformOwner);
        ((UserType)2).Should().Be(UserType.TenantOwner);
        ((UserType)3).Should().Be(UserType.IssueManager);
        ((UserType)4).Should().Be(UserType.IssueAssignee);
        ((UserType)5).Should().Be(UserType.ChatAgent);
        ((UserType)6).Should().Be(UserType.ChatSupervisor);
        ((UserType)7).Should().Be(UserType.EndUser);
        ((UserType)8).Should().Be(UserType.ApiConsumer);
    }

    [Test]
    public void UserType_IsDefined_ShouldWorkCorrectly()
    {
        // Assert
        Enum.IsDefined(typeof(UserType), 1).Should().BeTrue();
        Enum.IsDefined(typeof(UserType), 8).Should().BeTrue();
        Enum.IsDefined(typeof(UserType), 0).Should().BeFalse();
        Enum.IsDefined(typeof(UserType), 9).Should().BeFalse();
    }
}