namespace BankingApp.Application.Features.PasswordReset.Commands;

using Common.Validation;
using FluentValidation;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .Must(InputRules.IsValidEmail)
            .WithMessage("Invalid email format.");
    }
}
