namespace BankingApp.Application.Features.UserRegistration.Commands;

using System.Transactions;
using Common.Contracts;
using Common.Contracts.Security;
using Common.Logging;
using Common.Utilities;
using Domain.Aggregates.IdentityAggregate;
using Domain.Aggregates.UserAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed record RegisterCommand(string Email, string Password, string FullName)
    : IRequest<ErrorOr<Success>>;

public sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IIdentityRepository identityRepository,
    IHashService hashService,
    IUnitOfWork unitOfWork,
    ISystemClock clock,
    ILogger<RegisterCommandHandler> logger)
    : IRequestHandler<RegisterCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        ErrorOr<Email> emailResult = Email.Create(command.Email);
        if (emailResult.IsError)
        {
            return emailResult.FirstError;
        }

        ErrorOr<Success> emailAvailabilityResult = await EnsureEmailAvailableAsync(emailResult.Value, cancellationToken);
        if (emailAvailabilityResult.IsError)
        {
            return emailAvailabilityResult.FirstError;
        }

        ErrorOr<string> hashResult = CreatePasswordHash(command.Password);
        if (hashResult.IsError)
        {
            return hashResult.FirstError;
        }

        return await RegisterUserAsync(command, emailResult.Value, hashResult.Value, cancellationToken);
    }

    private async Task<ErrorOr<Success>> EnsureEmailAvailableAsync(Email email, CancellationToken cancellationToken)
    {
        User? existing = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (existing is not null)
        {
            logger.RegistrationRejectedEmailAlreadyRegistered();
            return AuthErrors.EmailAlreadyRegistered;
        }

        return Result.Success;
    }

    private ErrorOr<string> CreatePasswordHash(string password)
    {
        ErrorOr<string> hashResult = hashService.GetHash(password);
        if (hashResult.IsError)
        {
            logger.RegistrationHashGenerationFailed();
            return hashResult.FirstError;
        }

        return hashResult.Value;
    }

    private async Task<ErrorOr<Success>> RegisterUserAsync(
        RegisterCommand command,
        Email email,
        string passwordHash,
        CancellationToken cancellationToken)
    {
        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        DateTime now = clock.UtcNow;
        var user = User.Register(email, command.FullName.Trim(), now);
        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var identity = IdentityAccount.Create(user.Id, HashedPassword.Wrap(passwordHash));
        await identityRepository.AddAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        transactionScope.Complete();
        logger.UserRegisteredSuccessfully();
        return Result.Success;
    }
}

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .Must(InputRules.IsValidEmail)
            .WithMessage(AuthErrors.InvalidEmail.Description);

        RuleFor(command => command.Password)
            .Must(InputRules.IsStrongPassword)
            .WithMessage(ProfileErrors.WeakPassword.Description);

        RuleFor(command => command.FullName)
            .NotEmpty()
            .WithMessage(ProfileErrors.FullNameRequired.Description);
    }
}
