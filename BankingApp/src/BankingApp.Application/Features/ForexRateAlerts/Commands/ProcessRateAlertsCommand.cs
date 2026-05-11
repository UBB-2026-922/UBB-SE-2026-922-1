namespace BankingApp.Application.Features.ForexRateAlerts.Commands;

using Common.Contracts;
using Domain.Aggregates.RateAlertAggregate;
using Domain.Repositories;
using ErrorOr;
using MediatR;

public sealed record ProcessRateAlertsCommand : IRequest<ErrorOr<int>>;

public sealed class ProcessRateAlertsCommandHandler(
    IRateAlertRepository rateAlertRepository,
    IExchangeRateService exchangeRateService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ProcessRateAlertsCommand, ErrorOr<int>>
{
    public async Task<ErrorOr<int>> Handle(ProcessRateAlertsCommand command, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<RateAlert> alerts = await rateAlertRepository.ListAllUntriggeredAsync(cancellationToken);
        int triggered = 0;

        foreach (RateAlert alert in alerts)
        {
            ErrorOr<decimal> rateResult = exchangeRateService.GetRate(alert.BaseCurrency, alert.QuoteCurrency);
            if (rateResult.IsError)
            {
                continue;
            }

            if (!alert.ShouldTrigger(rateResult.Value))
            {
                continue;
            }

            alert.MarkTriggered();
            await rateAlertRepository.UpdateAsync(alert, cancellationToken);
            triggered++;
        }

        if (triggered > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return triggered;
    }
}
