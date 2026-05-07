namespace BankingApp.Domain.ValueObjects;

using BankingApp.Domain.Common.Primitives;
using BankingApp.Domain.Enums;
using BankingApp.Domain.Errors;
using ErrorOr;

public sealed record Money : ValueObject
{
    private Money()
    {
    }

    public decimal Amount { get; private init; }

    public Currency Currency { get; private init; }

    public static ErrorOr<Money> Create(decimal amount, Currency currency)
    {
        if (amount < 0)
        {
            return AccountErrors.NegativeAmount;
        }

        return new Money { Amount = amount, Currency = currency };
    }

    public static Money Zero(Currency currency) => new() { Amount = 0m, Currency = currency };

    public override string ToString() => $"{Amount} {Currency}";
}
