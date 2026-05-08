namespace BankingApp.Application.Features.PasswordReset.Commands;

using System.Security.Cryptography;
using System.Text;
using Common.Contracts;
using Common.Contracts.Notifications;
using Common.Logging;
using Common.Utilities;
using Domain.Aggregates.IdentityAggregate;
using Domain.Aggregates.UserAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed record ForgotPasswordCommand(string Email)
    : IRequest<ErrorOr<Success>>;

public sealed class ForgotPasswordCommandHandler(
    IUserRepository userRepository,
    IIdentityRepository identityRepository,
    IEmailService emailService,
    IUnitOfWork unitOfWork,
    ISystemClock clock,
    ILogger<ForgotPasswordCommandHandler> logger)
    : IRequestHandler<ForgotPasswordCommand, ErrorOr<Success>>
{
    private const int TokenExpiryMinutes = 30;
    private const int TokenByteLength = 32;

    public async Task<ErrorOr<Success>> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        ErrorOr<Email> emailResult = Email.Create(command.Email);
        if (emailResult.IsError)
        {
            logger.PasswordResetNoAccountFound();
            return Result.Success; // don't reveal whether account exists
        }

        User? user = await userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user is null)
        {
            logger.PasswordResetNoAccountFound();
            return Result.Success;
        }

        IdentityAccount? identity = await identityRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (identity is null)
        {
            return Result.Success;
        }

        byte[] randomBytes = RandomNumberGenerator.GetBytes(TokenByteLength);
        string rawToken = Convert.ToBase64String(randomBytes);
        string tokenHash = ComputeSha256Hash(rawToken);
        DateTime now = clock.UtcNow;

        identity.IssuePasswordResetToken(tokenHash, now.AddMinutes(TokenExpiryMinutes), now);
        await identityRepository.UpdateAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.PasswordResetEmailSent(user.Id);
        emailService.SendPasswordResetLink(user.Email.Value, rawToken);
        return Result.Success;
    }

    private static string ComputeSha256Hash(string rawData)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
