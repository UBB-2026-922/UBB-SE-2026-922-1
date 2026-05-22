namespace BankingApp.Application.Features.Authentication.Commands;

using Common.Notifications;
using Common.Security;
using Contracts.Features.Authentication.Dtos;
using Domain.Aggregates.IdentityAggregate;
using Domain.Aggregates.IdentityAggregate.Entities;
using Domain.Aggregates.UserAggregate;
using Domain.Common.Errors;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;
using Shared.Clock;
using Shared.Persistence;
using ApplicationLogMessages = Common.Logging.ApplicationLogMessages;

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
            ApplicationLogMessages.OtpVerificationUserNotFound(logger, command.UserId);
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
            ApplicationLogMessages.OtpVerificationFailed(logger, user.Id, verifyResult.FirstError.Description);
            return verifyResult.FirstError;
        }

        if (!verifyResult.Value)
        {
            ApplicationLogMessages.OtpVerificationInvalidOrExpired(logger, user.Id);
            if (otpAttemptTracker.RecordFailure(user.Id) < MaxFailedOtpAttempts)
            {
                return AuthErrors.InvalidOtp;
            }

            otpService.InvalidateOtp(user.Id);
            otpAttemptTracker.Reset(user.Id);
            ApplicationLogMessages.OtpChallengeInvalidated(logger, user.Id, MaxFailedOtpAttempts);
            return AuthErrors.OtpAttemptsExceeded;
        }

        otpAttemptTracker.Reset(user.Id);
        otpService.InvalidateOtp(user.Id);

        identity.ResetFailedAttempts();
        ErrorOr<string> tokenResult = jwtService.GenerateToken(user.Id);
        if (tokenResult.IsError)
        {
            ApplicationLogMessages.TokenGenerationFailed(logger, user.Id, tokenResult.FirstError.Description);
            return tokenResult.FirstError;
        }

        string token = tokenResult.Value;
        DateTime now = clock.UtcNow;
        SessionMetadata? metadata = command.Metadata;
        Session session = identity.OpenSession(token, now.AddHours(SessionExpiryHours), now, metadata?.DeviceInfo, metadata?.Browser, metadata?.IpAddress);

        await identityRepository.UpdateAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.UserLoggedIn(logger, user.Id);
        await emailService.SendLoginAlertAsync(user.Email.Value);
        return new FullLogin(user.Id, token, session.Id);
    }
}
