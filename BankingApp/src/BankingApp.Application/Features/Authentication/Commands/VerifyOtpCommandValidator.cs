namespace BankingApp.Application.Features.Authentication.Commands;

using Common.Validation;
using Domain.Common.Errors;
using FluentValidation;

public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(command => command.UserId).GreaterThan(0);
        RuleFor(command => command.OtpCode)
            .Must(InputRules.IsValidOtp)
            .WithMessage(AuthErrors.InvalidOtp.Description);
    }
}
