namespace BankingApp.Application.Features.ForexRateAlerts.Queries;

using Contracts.Features.ForexRateAlerts.Dtos;
using Domain.Aggregates.RateAlertAggregate;
using Domain.Repositories;
using ErrorOr;
using MediatR;

public sealed record GetRateAlertsQuery(int UserId) : IRequest<ErrorOr<List<ForexRateAlertDto>>>;

public sealed class GetRateAlertsQueryHandler(IRateAlertRepository rateAlertRepository)
    : IRequestHandler<GetRateAlertsQuery, ErrorOr<List<ForexRateAlertDto>>>
{
    public async Task<ErrorOr<List<ForexRateAlertDto>>> Handle(GetRateAlertsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<RateAlert> alerts = await rateAlertRepository.ListByUserIdAsync(query.UserId, cancellationToken);
        return alerts
            .OrderByDescending(rateAlert => rateAlert.CreatedAt)
            .Select(rateAlert => new ForexRateAlertDto
            {
                Id = rateAlert.Id,
                UserId = rateAlert.UserId,
                BaseCurrency = rateAlert.BaseCurrency.Code,
                TargetCurrency = rateAlert.QuoteCurrency.Code,
                TargetRate = rateAlert.TargetRate,
                IsBuyAlert = rateAlert.IsBuyAlert,
                IsTriggered = rateAlert.IsTriggered,
                CreatedAt = rateAlert.CreatedAt
            })
            .ToList();
    }
}
