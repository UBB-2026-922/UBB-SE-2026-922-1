// <copyright file="MockFactory.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.Dashboard;
using BankingApp.Application.Services.Login;
using BankingApp.Application.Services.PasswordRecovery;
using BankingApp.Application.Services.Profile;
using BankingApp.Application.Services.Registration;
using BankingApp.Application.Services.Security;

namespace BankingApp.Api.Tests.Integration.Infrastructure;

/// <summary>
///     Creates pre-configured Moq stubs for the service and repository interfaces
///     used across integration tests. All mocks use loose behaviour so individual
///     tests can override specific setups without affecting unrelated calls.
/// </summary>
public static class MockFactory
{
    /// <summary>Creates a loose mock for <see cref="IJsonWebTokenService" />.</summary>
    /// <returns>A new <see cref="Mock{T}" /> of <see cref="IJsonWebTokenService" />.</returns>
    public static Mock<IJsonWebTokenService> CreateJwtService()
    {
        return new Mock<IJsonWebTokenService>();
    }

    /// <summary>Creates a loose mock for <see cref="IAuthRepository" />.</summary>
    /// <returns>A new <see cref="Mock{T}" /> of <see cref="IAuthRepository" />.</returns>
    public static Mock<IAuthRepository> CreateAuthRepository()
    {
        return new Mock<IAuthRepository>();
    }

    /// <summary>Creates a loose mock for <see cref="ILoginService" />.</summary>
    /// <returns>A new <see cref="Mock{T}" /> of <see cref="ILoginService" />.</returns>
    public static Mock<ILoginService> CreateLoginService()
    {
        return new Mock<ILoginService>();
    }

    /// <summary>Creates a loose mock for <see cref="IRegistrationService" />.</summary>
    /// <returns>A new <see cref="Mock{T}" /> of <see cref="IRegistrationService" />.</returns>
    public static Mock<IRegistrationService> CreateRegistrationService()
    {
        return new Mock<IRegistrationService>();
    }

    /// <summary>Creates a loose mock for <see cref="IPasswordRecoveryService" />.</summary>
    /// <returns>A new <see cref="Mock{T}" /> of <see cref="IPasswordRecoveryService" />.</returns>
    public static Mock<IPasswordRecoveryService> CreatePasswordRecoveryService()
    {
        return new Mock<IPasswordRecoveryService>();
    }

    /// <summary>Creates a loose mock for <see cref="IDashboardService" />.</summary>
    /// <returns>A new <see cref="Mock{T}" /> of <see cref="IDashboardService" />.</returns>
    public static Mock<IDashboardService> CreateDashboardService()
    {
        return new Mock<IDashboardService>();
    }

    /// <summary>Creates a loose mock for <see cref="IProfileService" />.</summary>
    /// <returns>A new <see cref="Mock{T}" /> of <see cref="IProfileService" />.</returns>
    public static Mock<IProfileService> CreateProfileService()
    {
        return new Mock<IProfileService>();
    }
}