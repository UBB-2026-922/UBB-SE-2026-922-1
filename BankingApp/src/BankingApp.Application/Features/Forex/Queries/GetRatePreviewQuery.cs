namespace BankingApp.Application.Features.Forex.Queries;

using Common.Contracts;
using Common.Utilities;
using Domain.Common.Errors;
using Domain.Enums;
using Dtos;
using ErrorOr;
using MediatR;
using Currency = NodaMoney.Currency;

public sealed record GetRatePreviewQuery(
    int UserId,
    string SourceCurrency,
    string TargetCurrency,
    decimal SourceAmount)
    : IRequest<ErrorOr<ForexTransactionResponse>>;

public sealed class GetRatePreviewQueryHandler(
    IExchangeRateService exchangeRateService,
    ILockedRateCache lockedRateCache,
    ISystemClock clock)
    : IRequestHandler<GetRatePreviewQuery, ErrorOr<ForexTransactionResponse>>
{
    private const decimal CommissionRate = 0.005m;

    public Task<ErrorOr<ForexTransactionResponse>> Handle(GetRatePreviewQuery query, CancellationToken cancellationToken)
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
            return Task.FromResult<ErrorOr<ForexTransactionResponse>>(ForexErrors.InvalidCurrency);
        }

        if (sourceCurrency == targetCurrency)
        {
            return Task.FromResult<ErrorOr<ForexTransactionResponse>>(ForexErrors.SameCurrency);
        }

        ErrorOr<decimal> rateResult = exchangeRateService.GetRate(sourceCurrency, targetCurrency);
        if (rateResult.IsError)
        {
            return Task.FromResult<ErrorOr<ForexTransactionResponse>>(rateResult.FirstError);
        }

        decimal rate = rateResult.Value;
        decimal targetAmount = query.SourceAmount * rate;
        decimal commission = query.SourceAmount * CommissionRate;

        lockedRateCache.Store(query.UserId, sourceCurrency, targetCurrency, rate, clock.UtcNow);

        return Task.FromResult<ErrorOr<ForexTransactionResponse>>(new ForexTransactionResponse
        {
            Id = 0,
            SourceCurrency = sourceCurrency.Code,
            TargetCurrency = targetCurrency.Code,
            TargetAmount = Math.Round(targetAmount, 2),
            ExchangeRate = rate,
            Commission = Math.Round(commission, 2),
            Status = ExchangeTransactionStatus.Pending
        });
    }
}
