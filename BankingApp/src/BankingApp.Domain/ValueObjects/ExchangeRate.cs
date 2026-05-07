namespace BankingApp.Domain.ValueObjects;

using BankingApp.Domain.Common.Primitives;
using BankingApp.Domain.Enums;
using BankingApp.Domain.Errors;
using ErrorOr;

public sealed record ExchangeRate : ValueObject
{
    private ExchangeRate()
    {
    }

    public Currency BaseCurrency { get; private init; }

    public Currency QuoteCurrency { get; private init; }

    public decimal Value { get; private init; }

    public static ErrorOr<ExchangeRate> Create(Currency baseCurrency, Currency quoteCurrency, decimal value)
    {
        if (baseCurrency == quoteCurrency)
        {
            return ForexErrors.SameCurrency;
        }

        if (value <= 0)
        {
            return ForexErrors.InvalidRate;
        }

        return new ExchangeRate { BaseCurrency = baseCurrency, QuoteCurrency = quoteCurrency, Value = value };
    }

    public override string ToString() => $"{BaseCurrency}/{QuoteCurrency} @ {Value}";
}
