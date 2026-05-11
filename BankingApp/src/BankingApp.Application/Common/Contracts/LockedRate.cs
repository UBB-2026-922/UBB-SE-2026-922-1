namespace BankingApp.Application.Common.Contracts;

using Currency = NodaMoney.Currency;

public sealed record LockedRate(
    Currency BaseCurrency,
    Currency QuoteCurrency,
    decimal Rate,
    DateTime LockedAt);
