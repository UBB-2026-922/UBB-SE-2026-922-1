namespace BankingApp.Application.Features.ForexRateAlerts.Queries;

using Domain.Aggregates.RateAlertAggregate;
using Domain.Repositories;
using Dtos;
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
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new ForexRateAlertDto
            {
                Id = a.Id,
                UserId = a.UserId,
                BaseCurrency = a.BaseCurrency.Code,
                TargetCurrency = a.QuoteCurrency.Code,
                TargetRate = a.TargetRate,
                IsBuyAlert = a.IsBuyAlert,
                IsTriggered = a.IsTriggered,
                CreatedAt = a.CreatedAt
            })
            .ToList();
    }
}
