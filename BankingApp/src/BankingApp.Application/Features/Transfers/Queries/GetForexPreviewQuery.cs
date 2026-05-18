namespace BankingApp.Application.Features.Transfers.Queries;

using Contracts.Features.Transfers.Dtos;
using Domain.Common.Errors;
using ErrorOr;
using MediatR;
using Currency = NodaMoney.Currency;

public sealed record GetForexPreviewQuery(string SourceCurrency, string TargetCurrency, decimal Amount)
    : IRequest<ErrorOr<TransferForexPreviewResponse>>;

public sealed class GetForexPreviewQueryHandler(IExchangeRateService exchangeRateService)
    : IRequestHandler<GetForexPreviewQuery, ErrorOr<TransferForexPreviewResponse>>
{
    public Task<ErrorOr<TransferForexPreviewResponse>> Handle(GetForexPreviewQuery query,
        CancellationToken cancellationToken)
    {
        Currency source;
        Currency target;
        try
        {
            source = Currency.FromCode(query.SourceCurrency);
            target = Currency.FromCode(query.TargetCurrency);
        }
        catch
        {
            return Task.FromResult<ErrorOr<TransferForexPreviewResponse>>(ForexErrors.InvalidCurrency);
        }

        ErrorOr<decimal> rateResult = exchangeRateService.GetRate(source, target);
        if (rateResult.IsError)
        {
            return Task.FromResult<ErrorOr<TransferForexPreviewResponse>>(rateResult.FirstError);
        }

        decimal rate = rateResult.Value;
        return Task.FromResult<ErrorOr<TransferForexPreviewResponse>>(new TransferForexPreviewResponse
        {
            ExchangeRate = rate,
            ConvertedAmount = Math.Round(query.Amount * rate, 2)
        });
    }
}