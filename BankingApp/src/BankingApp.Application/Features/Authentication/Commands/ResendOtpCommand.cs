namespace BankingApp.Application.Features.Authentication.Commands;

using Common.Logging;
using Domain.Aggregates.IdentityAggregate;
using Domain.Aggregates.UserAggregate;
using Domain.Common.Errors;
using Domain.Enums;
using Domain.Repositories;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Notifications;
using Security;

public sealed record ResendOtpCommand(int UserId, string Method)
    : IRequest<ErrorOr<Success>>;

public sealed class ResendOtpCommandHandler(
    IUserRepository userRepository,
    IIdentityRepository identityRepository,
    IOtpService otpService,
    IOtpAttemptTracker otpAttemptTracker,
    IEmailService emailService,
    ILogger<ResendOtpCommandHandler> logger)
    : IRequestHandler<ResendOtpCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(ResendOtpCommand command, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            logger.OtpResendUserNotFound(command.UserId);
            return AuthErrors.UserNotFound;
        }

        IdentityAccount? identity = await identityRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (identity is null)
        {
            return AuthErrors.UserNotFound;
        }

        ErrorOr<TwoFactorMethod> requestedMethodResult = ParseRequestedMethod(command.Method);
        if (requestedMethodResult.IsError)
        {
            return requestedMethodResult.FirstError;
        }

        if (identity.Preferred2FaMethod != requestedMethodResult.Value)
        {
            return AuthErrors.InvalidTwoFactorMethod;
        }

        ErrorOr<string> otpResult = identity.Preferred2FaMethod == TwoFactorMethod.Authenticator
            ? otpService.GenerateTotp(user.Id)
            : otpService.GenerateSmsOtp(user.Id);

        if (otpResult.IsError)
        {
            logger.OtpGenerationDuringResendFailed(user.Id, otpResult.FirstError.Description);
            return otpResult.FirstError;
        }

        if (string.Equals(command.Method, nameof(TwoFactorMethod.Email), StringComparison.OrdinalIgnoreCase)
            || identity.Preferred2FaMethod == TwoFactorMethod.Email)
        {
            await emailService.SendOtpCodeAsync(user.Email.Value, otpResult.Value);
        }

        otpAttemptTracker.Reset(user.Id);
        return Result.Success;
    }

    private static ErrorOr<TwoFactorMethod> ParseRequestedMethod(string method)
    {
        if (Enum.TryParse<TwoFactorMethod>(method, true, out TwoFactorMethod parsedMethod))
        {
            return parsedMethod;
        }

        return AuthErrors.InvalidTwoFactorMethod;
    }
}

public sealed class ResendOtpCommandValidator : AbstractValidator<ResendOtpCommand>
{
    public ResendOtpCommandValidator()
    {
        RuleFor(command => command.UserId).GreaterThan(0);
        RuleFor(command => command.Method).NotEmpty();
    }
}
