namespace BankingApp.Domain.Services;

using BankingApp.Domain.Errors;
using ErrorOr;

public static class ExchangeRateCalculationsService
{
    public static ErrorOr<decimal> CalculateTargetAmount(decimal sourceAmount, decimal exchangeRate, decimal commission)
    {
        decimal result = sourceAmount * exchangeRate - commission;
        if (result < 0)
        {
            return ForexErrors.InvalidRate;
        }

        return result;
    }
}
