// <copyright file="MockFactory.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System.Security.Claims;
using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Notifications;
using BankApp.Application.Services.Security;
using BankApp.Domain.Entities;
using ErrorOr;

namespace BankApp.Application.Tests;

/// <summary>
///     Factory methods for creating Moq mocks with sensible default return values.
/// </summary>
internal static class MockFactory
{
    /// <summary>
    ///     Creates the configured CreateAuthRepository mock.
    /// </summary>
    /// <returns>The configured mock instance.</returns>
    internal static Mock<IAuthRepository> CreateAuthRepository()
    {
        var mock = new Mock<IAuthRepository>(MockBehavior.Strict);

        mock.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(It.IsAny<string>()))
            .Returns(Error.NotFound());
        mock.Setup(createsUser => createsUser.CreateUser(It.IsAny<User>()))
            .Returns(Result.Success);
        mock.Setup(findsOAuthLink => findsOAuthLink.FindOAuthLink(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Error.NotFound());
        mock.Setup(createsOAuthLink => createsOAuthLink.CreateOAuthLink(It.IsAny<OAuthLink>()))
            .Returns(Result.Success);
        mock.Setup(createsSession => createsSession.CreateSession(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>()))
            .Returns(new Session());
        mock.Setup(findsSessionByToken => findsSessionByToken.FindSessionByToken(It.IsAny<string>()))
            .Returns(Error.NotFound());
        mock.Setup(savesPasswordResetToken =>
                savesPasswordResetToken.SavePasswordResetToken(It.IsAny<PasswordResetToken>()))
            .Returns(Result.Success);
        mock.Setup(findsPasswordResetToken => findsPasswordResetToken.FindPasswordResetToken(It.IsAny<string>()))
            .Returns(Error.NotFound());
        mock.Setup(marksPasswordResetTokenAsUsed =>
                marksPasswordResetTokenAsUsed.MarkPasswordResetTokenAsUsed(It.IsAny<int>()))
            .Returns(Result.Success);
        mock.Setup(deletesExpiredPasswordResetTokens =>
                deletesExpiredPasswordResetTokens.DeleteExpiredPasswordResetTokens())
            .Returns(Result.Success);
        mock.Setup(invalidatesAllSessions => invalidatesAllSessions.InvalidateAllSessions(It.IsAny<int>()))
            .Returns(Result.Success);
        mock.Setup(findsUserById => findsUserById.FindUserById(It.IsAny<int>()))
            .Returns(Error.NotFound());
        mock.Setup(updatesPassword => updatesPassword.UpdatePassword(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Result.Success);
        mock.Setup(findsSessionsByUserId => findsSessionsByUserId.FindSessionsByUserId(It.IsAny<int>()))
            .Returns(new List<Session>());
        mock.Setup(updatesSessionToken => updatesSessionToken.UpdateSessionToken(It.IsAny<int>()))
            .Returns(Result.Success);
        mock.Setup(incrementsFailedAttempts => incrementsFailedAttempts.IncrementFailedAttempts(It.IsAny<int>()))
            .Returns(Result.Success);
        mock.Setup(resetsFailedAttempts => resetsFailedAttempts.ResetFailedAttempts(It.IsAny<int>()))
            .Returns(Result.Success);
        mock.Setup(locksAccount => locksAccount.LockAccount(It.IsAny<int>(), It.IsAny<DateTime>()))
            .Returns(Result.Success);

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
            .Returns((string input) => $"hashed_{input}");
        mock.Setup(verifies => verifies.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        return mock;
    }

    /// <summary>
    ///     Creates the configured CreateJwtService mock.
    /// </summary>
    /// <returns>The configured mock instance.</returns>
    internal static Mock<IJsonWebTokenService> CreateJwtService()
    {
        var mock = new Mock<IJsonWebTokenService>(MockBehavior.Strict);

        mock.Setup(generatesToken => generatesToken.GenerateToken(It.IsAny<int>()))
            .Returns("jwt-token");
        mock.Setup(validatesToken => validatesToken.ValidateToken(It.IsAny<string>()))
            .Returns(new ClaimsPrincipal());
        mock.Setup(extractsUserId => extractsUserId.ExtractUserId(It.IsAny<string>()))
            .Returns(0);

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
        mock.Setup(generatesSmsOtp => generatesSmsOtp.GenerateSmsOtp(It.IsAny<int>()))
            .Returns("123456");
        mock.Setup(verifiesSmsOtp => verifiesSmsOtp.VerifySmsOtp(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(true);
        mock.Setup(checksIsExpired => checksIsExpired.IsExpired(It.IsAny<DateTime>()))
            .Returns((DateTime expiresAt) => DateTime.UtcNow > expiresAt);
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