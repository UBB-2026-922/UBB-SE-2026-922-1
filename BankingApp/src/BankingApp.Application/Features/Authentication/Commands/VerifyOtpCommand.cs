namespace BankingApp.Application.Features.Authentication.Commands;

using Common.Contracts;
using Common.Contracts.Notifications;
using Common.Contracts.Security;
using Common.Logging;
using Common.Utilities;
using Domain.Aggregates.IdentityAggregate;
using Domain.Aggregates.UserAggregate;
using Domain.Common.Errors;
using Domain.Enums;
using Domain.Repositories;
using Dtos;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;

public sealed record VerifyOtpCommand(int UserId, string OtpCode, SessionMetadata? Metadata = null)
    : IRequest<ErrorOr<LoginSuccess>>;

public sealed class VerifyOtpCommandHandler(
    IUserRepository userRepository,
    IIdentityRepository identityRepository,
    IOtpService otpService,
    IOtpAttemptTracker otpAttemptTracker,
    IJsonWebTokenService jwtService,
    IEmailService emailService,
    IUnitOfWork unitOfWork,
    ISystemClock clock,
    ILogger<VerifyOtpCommandHandler> logger)
    : IRequestHandler<VerifyOtpCommand, ErrorOr<LoginSuccess>>
{
    private const int MaxFailedOtpAttempts = 3;
    private const int SessionExpiryHours = 24;

    public async Task<ErrorOr<LoginSuccess>> Handle(VerifyOtpCommand command, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            logger.OtpVerificationUserNotFound(command.UserId);
            return AuthErrors.UserNotFound;
        }

        IdentityAccount? identity = await identityRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (identity is null)
        {
            return AuthErrors.UserNotFound;
        }

        ErrorOr<bool> verifyResult = identity.Preferred2FaMethod == TwoFactorMethod.Authenticator
            ? otpService.VerifyTotp(user.Id, command.OtpCode)
            : otpService.VerifySmsOtp(user.Id, command.OtpCode);

        if (verifyResult.IsError)
        {
            logger.OtpVerificationFailed(user.Id, verifyResult.FirstError.Description);
            return verifyResult.FirstError;
        }

        if (!verifyResult.Value)
        {
            logger.OtpVerificationInvalidOrExpired(user.Id);
            if (otpAttemptTracker.RecordFailure(user.Id) < MaxFailedOtpAttempts)
            {
                return AuthErrors.InvalidOtp;
            }

            otpService.InvalidateOtp(user.Id);
            otpAttemptTracker.Reset(user.Id);
            logger.OtpChallengeInvalidated(user.Id, MaxFailedOtpAttempts);
            return AuthErrors.OtpAttemptsExceeded;
        }

        otpAttemptTracker.Reset(user.Id);
        otpService.InvalidateOtp(user.Id);

        identity.ResetFailedAttempts();
        ErrorOr<string> tokenResult = jwtService.GenerateToken(user.Id);
        if (tokenResult.IsError)
        {
            logger.TokenGenerationFailed(user.Id, tokenResult.FirstError.Description);
            return tokenResult.FirstError;
        }

        string token = tokenResult.Value;
        DateTime now = clock.UtcNow;
        SessionMetadata? metadata = command.Metadata;
        identity.OpenSession(token, now.AddHours(SessionExpiryHours), now, metadata?.DeviceInfo, metadata?.Browser, metadata?.IpAddress);

        await identityRepository.UpdateAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.UserLoggedIn(user.Id);
        emailService.SendLoginAlert(user.Email.Value);
        return new FullLogin(user.Id, token);
    }
}
