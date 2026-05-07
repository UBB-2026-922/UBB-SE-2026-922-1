namespace BankingApp.Domain.ValueObjects;

using BankingApp.Domain.Common.Primitives;
using BankingApp.Domain.Enums;
using BankingApp.Domain.Errors;
using ErrorOr;

public sealed record CurrencyPair : ValueObject
{
    private CurrencyPair()
    {
    }

    public Currency BaseCurrency { get; private init; }

    public Currency QuoteCurrency { get; private init; }

    public static ErrorOr<CurrencyPair> Create(Currency baseCurrency, Currency quoteCurrency)
    {
        if (baseCurrency == quoteCurrency)
        {
            return ForexErrors.SameCurrency;
        }

        return new CurrencyPair { BaseCurrency = baseCurrency, QuoteCurrency = quoteCurrency };
    }

    public override string ToString() => $"{BaseCurrency}/{QuoteCurrency}";
}
