namespace BankingApp.Application.Features.ForexRateAlerts.Commands;

using Domain.Common.Errors;
using FluentValidation;

public sealed class CreateRateAlertCommandValidator : AbstractValidator<CreateRateAlertCommand>
{
    public CreateRateAlertCommandValidator()
    {
        RuleFor(command => command.UserId).GreaterThan(0);
        RuleFor(command => command.BaseCurrency).Length(3);
        RuleFor(command => command.TargetCurrency).Length(3);
        RuleFor(command => command.TargetRate).GreaterThan(0m);
        RuleFor(command => command)
            .Must(command => !string.Equals(command.BaseCurrency, command.TargetCurrency, StringComparison.OrdinalIgnoreCase))
            .WithMessage(ForexErrors.SameCurrency.Description);
    }
}
