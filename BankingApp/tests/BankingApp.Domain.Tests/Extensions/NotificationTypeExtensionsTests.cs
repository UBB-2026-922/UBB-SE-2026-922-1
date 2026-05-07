// <copyright file="NotificationTypeExtensionsTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankingApp.Domain.Tests.Extensions;

using Enums;
using BankingApp.Domain.Extensions;

/// <summary>
///     Unit tests for <see cref="NotificationTypeExtensions" />.
/// </summary>
public class NotificationTypeExtensionsTests
{
    [Theory]
    [InlineData(NotificationType.InboundTransfer, "Inbound Transfer")]
    [InlineData(NotificationType.OutboundTransfer, "Outbound Transfer")]
    [InlineData(NotificationType.LowBalance, "Low Balance")]
    [InlineData(NotificationType.DuePayment, "Due Payment")]
    [InlineData(NotificationType.SuspiciousActivity, "Suspicious Activity")]
    public void ToDisplayName_WhenTypeHasCustomDisplayName_ReturnsCustomDisplayName(
        NotificationType notificationType,
        string expectedDisplayName)
    {
        // Act
        string displayName = notificationType.ToDisplayName();

        // Assert
        displayName.Should().Be(expectedDisplayName);
    }

    [Fact]
    public void ToDisplayName_WhenTypeUsesDefaultName_ReturnsEnumName()
    {
        // Arrange
        const NotificationType notificationType = NotificationType.Payment;

        // Act
        string displayName = notificationType.ToDisplayName();

        // Assert
        displayName.Should().Be("Payment");
    }

    [Theory]
    [InlineData("Payment", NotificationType.Payment)]
    [InlineData("Inbound Transfer", NotificationType.InboundTransfer)]
    [InlineData("Outbound Transfer", NotificationType.OutboundTransfer)]
    [InlineData("Low Balance", NotificationType.LowBalance)]
    [InlineData("Due Payment", NotificationType.DuePayment)]
    [InlineData("Suspicious Activity", NotificationType.SuspiciousActivity)]
    public void FromString_WhenDisplayNameIsKnown_ReturnsMatchingEnum(
        string displayName,
        NotificationType expectedNotificationType)
    {
        // Act
        NotificationType notificationType = NotificationTypeExtensions.FromString(displayName);

        // Assert
        notificationType.Should().Be(expectedNotificationType);
    }

    [Fact]
    public void FromString_WhenDisplayNameIsUnknown_ThrowsArgumentException()
    {
        // Arrange
        const string unknownDisplayName = "Unknown Display Name";

        // Act
        Action parseAction = () => NotificationTypeExtensions.FromString(unknownDisplayName);

        // Assert
        parseAction
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("*Unknown NotificationType:*");
    }
}
