namespace BankingApp.Application.Features.RecurringPayments.Commands;

using Domain.Common.Errors;
using FluentValidation;

public sealed class CreateRecurringPaymentCommandValidator : AbstractValidator<CreateRecurringPaymentCommand>
{
    public CreateRecurringPaymentCommandValidator()
    {
        RuleFor(command => command.UserId).GreaterThan(0);
        RuleFor(command => command.BillerId).GreaterThan(0);
        RuleFor(command => command.SourceAccountId).GreaterThan(0);
        RuleFor(command => command.Amount).GreaterThan(0m);
        RuleFor(command => command.EndDate)
            .Must((command, endDate) => endDate is null || endDate > command.StartDate)
            .WithMessage(RecurringPaymentErrors.InvalidEndDate.Description);
    }
}
