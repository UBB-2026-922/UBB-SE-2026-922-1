namespace BankingApp.Application.Features.Authentication.Commands;

using Common.Notifications;
using Common.Security;
using Contracts.Features.Authentication.Dtos;
using Domain.Aggregates.IdentityAggregate;
using Domain.Aggregates.UserAggregate;
using Domain.Common.Errors;
using Domain.Enums;
using Domain.Repositories;
using Domain.ValueObjects;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Models;
using Shared.Clock;
using Shared.Persistence;
using ApplicationLogMessages = Common.Logging.ApplicationLogMessages;

public sealed record LoginCommand(string Email, string Password, SessionMetadata? Metadata = null)
    : IRequest<ErrorOr<LoginSuccess>>;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IIdentityRepository identityRepository,
    IHashService hashService,
    IJsonWebTokenService jwtService,
    IOtpService otpService,
    IOtpAttemptTracker otpAttemptTracker,
    IEmailService emailService,
    IUnitOfWork unitOfWork,
    ISystemClock clock,
    ILogger<LoginCommandHandler> logger)
    : IRequestHandler<LoginCommand, ErrorOr<LoginSuccess>>
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;
    private const int SessionExpiryHours = 24;

    public async Task<ErrorOr<LoginSuccess>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        ErrorOr<Email> emailResult = Email.Create(command.Email);
        if (emailResult.IsError)
        {
            return AuthErrors.InvalidCredentials;
        }

        ErrorOr<(User User, IdentityAccount Identity)> loginContextResult =
            await LoadLoginContextAsync(emailResult.Value, cancellationToken);
        if (loginContextResult.IsError)
        {
            return loginContextResult.FirstError;
        }

        User user = loginContextResult.Value.User;
        IdentityAccount identity = loginContextResult.Value.Identity;

        if (identity.IsLocked)
        {
            if (identity.IsCurrentlyLocked())
            {
                ApplicationLogMessages.LoginBlockedLockedAccount(logger, user.Id, identity.LockoutEnd);
                return AuthErrors.AccountLocked;
            }

            identity.ResetFailedAttempts();
            await identityRepository.UpdateAsync(identity, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        if (identity.PasswordHash is null)
        {
            ApplicationLogMessages.LoginOAuthOnlyPasswordRejected(logger, user.Id);
            return AuthErrors.InvalidCredentials;
        }

        ErrorOr<bool> verifyResult = hashService.Verify(command.Password, identity.PasswordHash.Value);
        if (verifyResult.IsError)
        {
            ApplicationLogMessages.PasswordHashVerificationFailed(logger, user.Id, verifyResult.FirstError.Description);
            return verifyResult.FirstError;
        }

        if (!verifyResult.Value)
        {
            return await HandleFailedPasswordAsync(identity, user.Id, cancellationToken);
        }

        if (identity.Is2FaEnabled)
        {
            return await Handle2FaAsync(user, identity, cancellationToken);
        }

        return await CompleteLoginAsync(user, identity, command.Metadata, cancellationToken);
    }

    private async Task<ErrorOr<(User User, IdentityAccount Identity)>> LoadLoginContextAsync(
        Email email,
        CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            ApplicationLogMessages.LoginUserNotFoundForEmail(logger);
            return AuthErrors.InvalidCredentials;
        }

        IdentityAccount? identity = await identityRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (identity is null)
        {
            return AuthErrors.InvalidCredentials;
        }

        return (user, identity);
    }

    private async Task<Error> HandleFailedPasswordAsync(IdentityAccount identity, int userId, CancellationToken ct)
    {
        identity.IncrementFailedAttempts();
        int attempts = identity.FailedLoginAttempts;
        ApplicationLogMessages.FailedLoginAttempt(logger, userId, attempts, MaxFailedAttempts);

        if (attempts < MaxFailedAttempts)
        {
            await identityRepository.UpdateAsync(identity, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return AuthErrors.InvalidCredentials;
        }

        identity.LockAccount(clock.UtcNow.AddMinutes(LockoutMinutes));
        await identityRepository.UpdateAsync(identity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        ApplicationLogMessages.AccountLockedTooManyAttempts(logger, userId, LockoutMinutes, MaxFailedAttempts);
        return AuthErrors.AccountLockedTooManyAttempts;
    }

    private async Task<ErrorOr<LoginSuccess>> Handle2FaAsync(User user, IdentityAccount identity, CancellationToken _)
    {
        ErrorOr<string> otpResult = identity.Preferred2FaMethod == TwoFactorMethod.Authenticator
            ? otpService.GenerateTotp(user.Id)
            : otpService.GenerateSmsOtp(user.Id);

        if (otpResult.IsError)
        {
            ApplicationLogMessages.OtpGenerationFailed(logger, user.Id, otpResult.FirstError.Description);
            return otpResult.FirstError;
        }

        if (identity.Preferred2FaMethod == TwoFactorMethod.Email)
        {
            await emailService.SendOtpCodeAsync(user.Email.Value, otpResult.Value);
        }

        otpAttemptTracker.Reset(user.Id);
        ApplicationLogMessages.TwoFactorRequired(logger, user.Id, identity.Preferred2FaMethod);
        return new RequiresTwoFactor(user.Id);
    }

    private async Task<ErrorOr<LoginSuccess>> CompleteLoginAsync(
        User user,
        IdentityAccount identity,
        SessionMetadata? metadata,
        CancellationToken ct)
    {
        identity.ResetFailedAttempts();

        ErrorOr<string> tokenResult = jwtService.GenerateToken(user.Id);
        if (tokenResult.IsError)
        {
            ApplicationLogMessages.TokenGenerationFailed(logger, user.Id, tokenResult.FirstError.Description);
            return tokenResult.FirstError;
        }

        string token = tokenResult.Value;
        DateTime now = clock.UtcNow;
        identity.OpenSession(token, now.AddHours(SessionExpiryHours), now, metadata?.DeviceInfo, metadata?.Browser, metadata?.IpAddress);

        await identityRepository.UpdateAsync(identity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        ApplicationLogMessages.UserLoggedIn(logger, user.Id);
        await emailService.SendLoginAlertAsync(user.Email.Value);
        return new FullLogin(user.Id, token);
    }
}

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage(AuthErrors.InvalidEmail.Description);

        RuleFor(command => command.Password).NotEmpty();
    }
}
