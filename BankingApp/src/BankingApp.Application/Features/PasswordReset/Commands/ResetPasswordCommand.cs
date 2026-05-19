namespace BankingApp.Application.Features.PasswordReset.Commands;

using System.Security.Cryptography;
using System.Text;
using Common.Security;
using Common.Validation;
using Domain.Aggregates.IdentityAggregate;
using Domain.Aggregates.IdentityAggregate.Entities;
using Domain.Common.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Clock;
using Shared.Persistence;
using ApplicationLogMessages = Common.Logging.ApplicationLogMessages;

public sealed record ResetPasswordCommand(string Token, string NewPassword)
    : IRequest<ErrorOr<Success>>;

public sealed class ResetPasswordCommandHandler(
    IIdentityRepository identityRepository,
    IHashService hashService,
    IUnitOfWork unitOfWork,
    ISystemClock clock,
    ILogger<ResetPasswordCommandHandler> logger)
    : IRequestHandler<ResetPasswordCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        string tokenHash = ComputeSha256Hash(command.Token);
        IdentityAccount? identity = await identityRepository.GetByResetTokenHashAsync(tokenHash, cancellationToken);
        if (identity is null)
        {
            ApplicationLogMessages.PasswordResetTokenNotFound(logger);
            return PasswordResetErrors.TokenInvalid;
        }

        PasswordResetToken? resetToken = identity.PasswordResetTokens.FirstOrDefault(t => t.TokenHash == tokenHash);
        if (resetToken is null)
        {
            return PasswordResetErrors.TokenInvalid;
        }

        if (resetToken.UsedAt is not null)
        {
            ApplicationLogMessages.PasswordResetValidationFailed(logger, identity.UserId, PasswordResetErrors.TokenAlreadyUsed.Code);
            return PasswordResetErrors.TokenAlreadyUsed;
        }

        DateTime now = clock.UtcNow;
        if (resetToken.ExpiresAt < now)
        {
            ApplicationLogMessages.PasswordResetValidationFailed(logger, identity.UserId, PasswordResetErrors.TokenExpired.Code);
            return PasswordResetErrors.TokenExpired;
        }

        ErrorOr<string> hashResult = hashService.GetHash(command.NewPassword);
        if (hashResult.IsError)
        {
            ApplicationLogMessages.PasswordResetHashGenerationFailed(logger, identity.UserId);
            return hashResult.FirstError;
        }

        identity.UpdatePassword(HashedPassword.Wrap(hashResult.Value));
        resetToken.MarkUsed(now);
        identity.InvalidateAllSessions();

        await identityRepository.UpdateAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.PasswordResetSucceeded(logger, identity.UserId);
        return Result.Success;
    }

    private static string ComputeSha256Hash(string rawData)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(command => command.Token)
            .NotEmpty()
            .WithMessage(PasswordResetErrors.TokenInvalid.Description);

        RuleFor(command => command.NewPassword)
            .Must(InputRules.IsStrongPassword)
            .WithMessage(ProfileErrors.WeakPassword.Description);
    }
}
