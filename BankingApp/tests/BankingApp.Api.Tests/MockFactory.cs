// <copyright file="MockFactory.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankingApp.Application.DataTransferObjects.Auth;
using BankingApp.Application.DataTransferObjects.Dashboard;
using BankingApp.Application.DataTransferObjects.Profile;
using BankingApp.Application.Enums;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.Dashboard;
using BankingApp.Application.Services.Login;
using BankingApp.Application.Services.Notifications;
using BankingApp.Application.Services.PasswordRecovery;
using BankingApp.Application.Services.Profile;
using BankingApp.Application.Services.Registration;
using BankingApp.Application.Services.Security;
using BankingApp.Domain.Entities;
using ErrorOr;

namespace BankingApp.Api.Tests;

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
        mock.Setup(oauthLoginAsync => oauthLoginAsync.OAuthLoginAsync(
                It.IsAny<OAuthLoginRequest>(),
                It.IsAny<SessionMetadata?>()))
            .ReturnsAsync((ErrorOr<LoginSuccess>)new FullLogin(0, string.Empty));
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
        mock.Setup(oauthRegister => oauthRegister.OAuthRegister(It.IsAny<OAuthRegisterRequest>()))
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
            .Returns(new DashboardResponse());
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
            .Returns(new ProfileInfo());
        mock.Setup(updatesPersonalInfo => updatesPersonalInfo.UpdatePersonalInfo(It.IsAny<UpdateProfileRequest>()))
            .Returns(Result.Success);
        mock.Setup(changesPassword => changesPassword.ChangePassword(It.IsAny<ChangePasswordRequest>()))
            .Returns(Result.Success);
        mock.Setup(enables2Fa => enables2Fa.Enable2Fa(It.IsAny<int>(), It.IsAny<TwoFactorMethod>()))
            .Returns(Result.Success);
        mock.Setup(disables2Fa => disables2Fa.Disable2Fa(It.IsAny<int>()))
            .Returns(Result.Success);
        mock.Setup(getsOAuthLinks => getsOAuthLinks.GetOAuthLinks(It.IsAny<int>()))
            .Returns(new List<OAuthLinkDataTransferObject>());
        mock.Setup(linksOAuth => linksOAuth.LinkOAuth(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Result.Success);
        mock.Setup(unlinksOAuth => unlinksOAuth.UnlinkOAuth(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Result.Success);
        mock.Setup(getsNotificationPreferences =>
                getsNotificationPreferences.GetNotificationPreferences(It.IsAny<int>()))
            .Returns(new List<NotificationPreferenceDataTransferObject>());
        mock.Setup(updatesNotificationPreferences => updatesNotificationPreferences.UpdateNotificationPreferences(
                It.IsAny<int>(),
                It.IsAny<List<NotificationPreferenceDataTransferObject>>()))
            .Returns(Result.Success);
        mock.Setup(verifiesPassword => verifiesPassword.VerifyPassword(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(true);
        mock.Setup(getsActiveSessions => getsActiveSessions.GetActiveSessions(It.IsAny<int>()))
            .Returns(new List<SessionDataTransferObject>());
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
