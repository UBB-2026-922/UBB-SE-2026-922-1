// <copyright file="OtpServiceTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankingApp.Infrastructure.Services.Security;

namespace BankingApp.Infrastructure.Tests.Services.Security;

/// <summary>
///     Regression tests ensuring <see cref="OtpService" /> refuses construction with an absent or
///     invalid secret and never falls back to a known default value.
///     These tests exist to prevent anyone from re-introducing a fallback secret constant.
/// </summary>
public class OtpServiceTests
{
    [Fact]
    public void Constructor_WhenSecretIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        const string otpServerSecret = "";

        // Act
        Action otpServiceInstantiation = () =>
        {
            _ = new OtpService(otpServerSecret);
        };

        // Assert
        otpServiceInstantiation.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenSecretIsWhitespace_ThrowsArgumentException()
    {
        // Arrange
        const string otpServerSecret = "   ";

        // Act
        Action otpServiceInstantiation = () =>
        {
            _ = new OtpService(otpServerSecret);
        };

        // Assert
        otpServiceInstantiation.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenSecretIsValid_DoesNotThrow()
    {
        // Arrange
        const string otpServerSecret = "any-placeholder-otp-secret";

        // Act
        Action otpServiceInstantiation = () =>
        {
            _ = new OtpService(otpServerSecret);
        };

        // Assert
        otpServiceInstantiation.Should().NotThrow();
    }
}
