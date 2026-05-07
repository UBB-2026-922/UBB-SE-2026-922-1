namespace BankingApp.Api.Tests;

using Application.Repositories.Interfaces;
using Application.Services.Dashboard;
using Application.Services.Login;
using Application.Services.Notifications;
using Application.Services.PasswordRecovery;
using Application.Services.Profile;
using Application.Services.Registration;
using Application.Services.Security;
using Domain.Entities;
using Domain.Enums;
using ErrorOr;
using Application.DTOs.Auth;
using Application.DTOs.Dashboard;
using Application.DTOs.Profile;

/// <summary>
///     Factory methods for creating Moq mocks with sensible default return values.
/// </summary>
internal static class MockFactory
{
    /// <summary>
    ///     Creates the configured CreateLoginService mock.
    /// </summary>
    /// <returns>The configured mock instance.</returns>
    internal static Mock<ILoginService> CreateLoginService()
    {
        var mock = new Mock<ILoginService>(MockBehavior.Strict);
        mock.Setup(login => login.Login(It.IsAny<LoginRequest>(), It.IsAny<SessionMetadata?>()))
            .Returns((ErrorOr<LoginSuccess>)new FullLogin(0, string.Empty));
        mock.Setup(verifiesOtp => verifiesOtp.VerifyOtp(It.IsAny<VerifyOtpRequest>(), It.IsAny<SessionMetadata?>()))
            .Returns((ErrorOr<LoginSuccess>)new FullLogin(0, string.Empty));
        mock.Setup(logout => logout.Logout(It.IsAny<string>()))
            .Returns(Result.Success);
        mock.Setup(resendOtp => resendOtp.ResendOtp(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Result.Success);
        return mock;
    }

    /// <summary>
    ///     Creates the configured CreateRegistrationService mock.
    /// </summary>
    /// <returns>The configured mock instance.</returns>
    internal static Mock<IRegistrationService> CreateRegistrationService()
    {
        var mock = new Mock<IRegistrationService>(MockBehavior.Strict);
        mock.Setup(register => register.Register(It.IsAny<RegisterRequest>()))
            .Returns(Result.Success);
        return mock;
    }

    /// <summary>
    ///     Creates the configured CreatePasswordRecoveryService mock.
    /// </summary>
    /// <returns>The configured mock instance.</returns>
    internal static Mock<IPasswordRecoveryService> CreatePasswordRecoveryService()
    {
        var mock = new Mock<IPasswordRecoveryService>(MockBehavior.Strict);
        mock.Setup(requestsPasswordReset => requestsPasswordReset.RequestPasswordReset(It.IsAny<string>()))
            .Returns(Result.Success);
        mock.Setup(resetsPassword => resetsPassword.ResetPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Result.Success);
        mock.Setup(verifiesResetToken => verifiesResetToken.VerifyResetToken(It.IsAny<string>()))
            .Returns(Result.Success);
        return mock;
    }

    /// <summary>
    ///     Creates the configured CreateDashboardService mock.
    /// </summary>
    /// <returns>The configured mock instance.</returns>
    internal static Mock<IDashboardService> CreateDashboardService()
    {
        var mock = new Mock<IDashboardService>(MockBehavior.Strict);
        mock.Setup(getsDashboardData => getsDashboardData.GetDashboardData(It.IsAny<int>()))
            .Returns(new DashboardDto());
        return mock;
    }

    /// <summary>
    ///     Creates the configured CreateProfileService mock.
    /// </summary>
    /// <returns>The configured mock instance.</returns>
    internal static Mock<IProfileService> CreateProfileService()
    {
        var mock = new Mock<IProfileService>(MockBehavior.Strict);
        mock.Setup(getsProfile => getsProfile.GetProfile(It.IsAny<int>()))
            .Returns(new ProfileDto());
        mock.Setup(updatesPersonalInfo => updatesPersonalInfo.UpdatePersonalInfo(It.IsAny<UpdateProfileRequest>()))
            .Returns(Result.Success);
        mock.Setup(changesPassword => changesPassword.ChangePassword(It.IsAny<ChangePasswordRequest>()))
            .Returns(Result.Success);
        mock.Setup(enables2Fa => enables2Fa.Enable2Fa(It.IsAny<int>(), It.IsAny<TwoFactorMethod>()))
            .Returns(Result.Success);
        mock.Setup(disables2Fa => disables2Fa.Disable2Fa(It.IsAny<int>()))
            .Returns(Result.Success);
        mock.Setup(getsNotificationPreferences =>
                getsNotificationPreferences.GetNotificationPreferences(It.IsAny<int>()))
            .Returns(new List<NotificationPreferenceDto>());
        mock.Setup(updatesNotificationPreferences => updatesNotificationPreferences.UpdateNotificationPreferences(
                It.IsAny<int>(),
                It.IsAny<List<NotificationPreferenceDto>>()))
            .Returns(Result.Success);
        mock.Setup(verifiesPassword => verifiesPassword.VerifyPassword(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(true);
        mock.Setup(getsActiveSessions => getsActiveSessions.GetActiveSessions(It.IsAny<int>()))
            .Returns(new List<SessionDto>());
        mock.Setup(revokesSession => revokesSession.RevokeSession(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Result.Success);
        return mock;
    }

    /// <summary>
    ///     Creates the configured CreateJwtService mock.
    /// </summary>
    /// <returns>The configured mock instance.</returns>
    internal static Mock<IJsonWebTokenService> CreateJwtService()
    {
        var mock = new Mock<IJsonWebTokenService>(MockBehavior.Strict);
        mock.Setup(extractsUserId => extractsUserId.ExtractUserId(It.IsAny<string>()))
            .Returns(0);
        return mock;
    }

    /// <summary>
    ///     Creates the configured CreateAuthRepository mock.
    /// </summary>
    /// <returns>The configured mock instance.</returns>
    internal static Mock<IAuthRepository> CreateAuthRepository()
    {
        var mock = new Mock<IAuthRepository>(MockBehavior.Strict);
        mock.Setup(findsSessionByToken => findsSessionByToken.FindSessionByToken(It.IsAny<string>()))
            .Returns(new Session());
        mock.Setup(checksSession => checksSession.IsSessionActive(It.IsAny<string>()))
            .Returns(true);
        return mock;
    }

    /// <summary>
    ///     Creates the configured CreateHashService mock.
    /// </summary>
    /// <returns>The configured mock instance.</returns>
    internal static Mock<IHashService> CreateHashService()
    {
        var mock = new Mock<IHashService>(MockBehavior.Strict);

        mock.Setup(getsHash => getsHash.GetHash(It.IsAny<string>()))
            .Returns((string input) => ErrorOrFactory.From($"hashed_{input}"));

        mock.Setup(verifies => verifies.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        return mock;
    }

    /// <summary>
    ///     Creates the configured CreateOtpService mock.
    /// </summary>
    /// <returns>The configured mock instance.</returns>
    internal static Mock<IOtpService> CreateOtpService()
    {
        var mock = new Mock<IOtpService>(MockBehavior.Strict);

        mock.Setup(generatesTotp => generatesTotp.GenerateTotp(It.IsAny<int>()))
            .Returns("123456");

        mock.Setup(verifiesTotp => verifiesTotp.VerifyTotp(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(true);

        mock.Setup(invalidatesOtp => invalidatesOtp.InvalidateOtp(It.IsAny<int>()));

        return mock;
    }

    /// <summary>
    ///     Creates the configured CreateEmailService mock.
    /// </summary>
    /// <returns>The configured mock instance.</returns>
    internal static Mock<IEmailService> CreateEmailService()
    {
        var mock = new Mock<IEmailService>(MockBehavior.Strict);

        mock.Setup(sendsOtpCode => sendsOtpCode.SendOtpCode(It.IsAny<string>(), It.IsAny<string>()));
        mock.Setup(sendsPasswordResetLink =>
            sendsPasswordResetLink.SendPasswordResetLink(It.IsAny<string>(), It.IsAny<string>()));
        mock.Setup(sendsLockNotification => sendsLockNotification.SendLockNotification(It.IsAny<string>()));
        mock.Setup(sendsLoginAlert => sendsLoginAlert.SendLoginAlert(It.IsAny<string>()));

        return mock;
    }
}