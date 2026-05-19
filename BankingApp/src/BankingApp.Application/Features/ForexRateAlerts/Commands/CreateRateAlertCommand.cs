namespace BankingApp.Application.Features.ForexRateAlerts.Commands;

using Contracts.Features.ForexRateAlerts.Dtos;
using Domain.Aggregates.RateAlertAggregate;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using Currency = NodaMoney.Currency;

public sealed record CreateRateAlertCommand(
    int UserId,
    string BaseCurrency,
    string TargetCurrency,
    decimal TargetRate,
    bool IsBuyAlert)
    : IRequest<ErrorOr<ForexRateAlertDto>>;

public sealed class CreateRateAlertCommandHandler(
    IRateAlertRepository rateAlertRepository,
    IUnitOfWork unitOfWork,
    ISystemClock clock)
    : IRequestHandler<CreateRateAlertCommand, ErrorOr<ForexRateAlertDto>>
{
    public async Task<ErrorOr<ForexRateAlertDto>> Handle(CreateRateAlertCommand command,
        CancellationToken cancellationToken)
    {
        Currency baseCurrency;
        Currency quoteCurrency;
        try
        {
            baseCurrency = Currency.FromCode(command.BaseCurrency);
            quoteCurrency = Currency.FromCode(command.TargetCurrency);
        }
        catch
        {
            return ForexErrors.InvalidCurrency;
        }

        ErrorOr<RateAlert> alertResult = RateAlert.Create(
            command.UserId,
            baseCurrency,
            quoteCurrency,
            command.TargetRate,
            command.IsBuyAlert,
            clock.UtcNow);

        if (alertResult.IsError)
        {
            return alertResult.FirstError;
        }

        RateAlert alert = alertResult.Value;
        await rateAlertRepository.AddAsync(alert, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ForexRateAlertDto
        {
            Id = alert.Id,
            UserId = alert.UserId,
            BaseCurrency = alert.BaseCurrency.Code,
            TargetCurrency = alert.QuoteCurrency.Code,
            TargetRate = alert.TargetRate,
            IsBuyAlert = alert.IsBuyAlert,
            IsTriggered = alert.IsTriggered,
            CreatedAt = alert.CreatedAt
        };
    }
}