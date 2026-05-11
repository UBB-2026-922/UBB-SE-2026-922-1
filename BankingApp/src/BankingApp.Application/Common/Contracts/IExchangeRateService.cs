namespace BankingApp.Application.Common.Contracts;

using ErrorOr;
using Currency = NodaMoney.Currency;

public interface IExchangeRateService
{
    public ErrorOr<decimal> GetRate(Currency baseCurrency, Currency quoteCurrency);
}
