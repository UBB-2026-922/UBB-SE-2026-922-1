namespace BankingApp.Application.Features.UserProfile.Commands;

using Common.Security;
using Common.Validation;
using Domain.Aggregates.IdentityAggregate;
using Domain.Aggregates.UserAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Persistence;
using ApplicationLogMessages = Common.Logging.ApplicationLogMessages;

public sealed record ChangePasswordCommand(int UserId, string CurrentPassword, string NewPassword)
    : IRequest<ErrorOr<Success>>;

public sealed class ChangePasswordCommandHandler(
    IUserRepository userRepository,
    IIdentityRepository identityRepository,
    IHashService hashService,
    IUnitOfWork unitOfWork,
    ILogger<ChangePasswordCommandHandler> logger)
    : IRequestHandler<ChangePasswordCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            ApplicationLogMessages.PasswordChangeUserNotFound(logger, command.UserId);
            return UserErrors.NotFound;
        }

        IdentityAccount? identity = await identityRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (identity is null)
        {
            return UserErrors.NotFound;
        }

        if (identity.PasswordHash is null)
        {
            ApplicationLogMessages.PasswordChangeOAuthOnlyRejected(logger, user.Id);
            return ProfileErrors.IncorrectPassword;
        }

        ErrorOr<bool> verifyResult = hashService.Verify(command.CurrentPassword, identity.PasswordHash.Value);
        if (verifyResult.IsError)
        {
            ApplicationLogMessages.PasswordChangeHashVerificationFailed(logger, user.Id);
            return verifyResult.FirstError;
        }

        if (!verifyResult.Value)
        {
            ApplicationLogMessages.PasswordChangeIncorrectCurrentPassword(logger, user.Id);
            return ProfileErrors.IncorrectPassword;
        }

        ErrorOr<string> newHashResult = hashService.GetHash(command.NewPassword);
        if (newHashResult.IsError)
        {
            ApplicationLogMessages.PasswordChangeHashGenerationFailed(logger, user.Id);
            return newHashResult.FirstError;
        }

        identity.UpdatePassword(HashedPassword.Wrap(newHashResult.Value));
        await identityRepository.UpdateAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ApplicationLogMessages.PasswordChangedSuccessfully(logger, user.Id);
        return Result.Success;
    }
}

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(command => command.UserId).GreaterThan(0);
        RuleFor(command => command.CurrentPassword).NotEmpty();
        RuleFor(command => command.NewPassword)
            .Must(InputRules.IsStrongPassword)
            .WithMessage(ProfileErrors.WeakPasswordChange.Description);
        RuleFor(command => command)
            .Must(command => command.CurrentPassword != command.NewPassword)
            .WithMessage("New password must be different from the current password.");
    }
}
