namespace BankingApp.Api.Tests.Integration.Infrastructure;

using BankingApp.Application.Features.UserProfile.Repositories;
using BankingApp.Application.Features.Beneficiaries.Services;
using BankingApp.Application.Features.AccountOverview.Services;
using BankingApp.Application.Features.Authentication.Services;
using BankingApp.Application.Features.PasswordReset.Services;
using BankingApp.Application.Features.UserProfile.Services;
using BankingApp.Application.Features.UserRegistration.Services;
using BankingApp.Application.Common.Security;

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

    /// <summary>Creates a loose mock for <see cref="IAuthenticationRepository" />.</summary>
    /// <returns>A new <see cref="Mock{T}" /> of <see cref="IAuthenticationRepository" />.</returns>
    public static Mock<IAuthenticationRepository> CreateAuthRepository()
    {
        return new Mock<IAuthenticationRepository>();
    }

    /// <summary>Creates a loose mock for <see cref="ILoginService" />.</summary>
    /// <returns>A new <see cref="Mock{T}" /> of <see cref="ILoginService" />.</returns>
    public static Mock<ILoginService> CreateLoginService()
    {
        return new Mock<ILoginService>();
    }

    /// <summary>Creates a loose mock for <see cref="IUserRegistrationService" />.</summary>
    /// <returns>A new <see cref="Mock{T}" /> of <see cref="IUserRegistrationService" />.</returns>
    public static Mock<IUserRegistrationService> CreateUserRegistrationService()
    {
        return new Mock<IUserRegistrationService>();
    }

    /// <summary>Creates a loose mock for <see cref="IPasswordResetService" />.</summary>
    /// <returns>A new <see cref="Mock{T}" /> of <see cref="IPasswordResetService" />.</returns>
    public static Mock<IPasswordResetService> CreatePasswordResetService()
    {
        return new Mock<IPasswordResetService>();
    }

    /// <summary>Creates a loose mock for <see cref="IAccountOverviewService" />.</summary>
    /// <returns>A new <see cref="Mock{T}" /> of <see cref="IAccountOverviewService" />.</returns>
    public static Mock<IAccountOverviewService> CreateAccountOverviewService()
    {
        return new Mock<IAccountOverviewService>();
    }

    /// <summary>Creates a loose mock for <see cref="IUserProfileService" />.</summary>
    /// <returns>A new <see cref="Mock{T}" /> of <see cref="IUserProfileService" />.</returns>
    public static Mock<IUserProfileService> CreateUserProfileService()
    {
        return new Mock<IUserProfileService>();
    }

    /// <summary>Creates a loose mock for <see cref="IBeneficiaryService" />.</summary>
    /// <returns>A new <see cref="Mock{T}" /> of <see cref="IBeneficiaryService" />.</returns>
    public static Mock<IBeneficiaryService> CreateBeneficiaryService()
    {
        return new Mock<IBeneficiaryService>();
    }
}
