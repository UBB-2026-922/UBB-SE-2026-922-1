namespace BankingApp.Domain.Errors;

using ErrorOr;

public static class ForexErrors
{
    public static readonly Error InvalidRate =
        Error.Validation("forex.invalid_rate", "The exchange rate produces a negative result.");

    public static readonly Error RateExpired =
        Error.Conflict("forex.rate_expired", "The locked exchange rate has expired.");

    public static readonly Error SameCurrency =
        Error.Validation("forex.same_currency", "Source and target currencies must be different.");

    public static readonly Error TransactionNotFound =
        Error.NotFound("forex.transaction_not_found", "Forex transaction was not found.");
}
