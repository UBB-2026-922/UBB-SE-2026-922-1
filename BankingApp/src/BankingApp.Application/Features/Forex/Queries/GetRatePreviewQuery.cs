namespace BankingApp.Application.Features.Forex.Queries;

using Contracts.Features.Forex.Dtos;
using Domain.Common.Errors;
using ErrorOr;
using MediatR;
using Currency = NodaMoney.Currency;

public sealed record GetRatePreviewQuery(
    int UserId,
    string SourceCurrency,
    string TargetCurrency,
    decimal SourceAmount)
    : IRequest<ErrorOr<ForexRatePreviewResponse>>;

public sealed class GetRatePreviewQueryHandler(
    IExchangeRateService exchangeRateService,
    ILockedRateCache lockedRateCache,
    ISystemClock clock)
    : IRequestHandler<GetRatePreviewQuery, ErrorOr<ForexRatePreviewResponse>>
{
    public Task<ErrorOr<ForexRatePreviewResponse>> Handle(GetRatePreviewQuery query,
        CancellationToken cancellationToken)
    {
        Currency sourceCurrency;
        Currency targetCurrency;
        try
        {
            sourceCurrency = Currency.FromCode(query.SourceCurrency);
            targetCurrency = Currency.FromCode(query.TargetCurrency);
        }
        catch
        {
            return Task.FromResult<ErrorOr<ForexRatePreviewResponse>>(ForexErrors.InvalidCurrency);
        }

        if (sourceCurrency == targetCurrency)
        {
            return Task.FromResult<ErrorOr<ForexRatePreviewResponse>>(ForexErrors.SameCurrency);
        }

        ErrorOr<decimal> rateResult = exchangeRateService.GetRate(sourceCurrency, targetCurrency);
        if (rateResult.IsError)
        {
            return Task.FromResult<ErrorOr<ForexRatePreviewResponse>>(rateResult.FirstError);
        }

        decimal rate = rateResult.Value;
        lockedRateCache.Store(query.UserId, sourceCurrency, targetCurrency, rate, clock.UtcNow);

        return Task.FromResult<ErrorOr<ForexRatePreviewResponse>>(new ForexRatePreviewResponse
        {
            SourceCurrency = sourceCurrency.Code,
            TargetCurrency = targetCurrency.Code,
            TargetAmount = Math.Round(query.SourceAmount * rate, 2),
            ExchangeRate = rate,
            Commission = Math.Round(query.SourceAmount * ForexPolicy.CommissionRate, 2)
        });
    }
}