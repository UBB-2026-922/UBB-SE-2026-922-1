namespace BankingApp.Domain.Aggregates.RateAlertAggregate;

using Common.Errors;
using Common.Primitives;
using ErrorOr;
using Currency = NodaMoney.Currency;

public sealed class RateAlert : AggregateRoot<int>
{
    private const int RatePrecisionDecimals = 2;

    private RateAlert()
    {
    }

    public int UserId { get; private set; }

    public Currency BaseCurrency { get; private set; }

    public Currency QuoteCurrency { get; private set; }

    public decimal TargetRate { get; private set; }

    public bool IsTriggered { get; private set; }

    public bool IsBuyAlert { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static ErrorOr<RateAlert> Create(
        int userId,
        Currency baseCurrency,
        Currency quoteCurrency,
        decimal targetRate,
        bool isBuyAlert,
        DateTime createdAt)
    {
        if (baseCurrency == quoteCurrency)
        {
            return RateAlertErrors.MatchingCurrencies;
        }

        if (targetRate <= 0)
        {
            return RateAlertErrors.InvalidTargetRate;
        }

        return new RateAlert
        {
            UserId = userId,
            BaseCurrency = baseCurrency,
            QuoteCurrency = quoteCurrency,
            TargetRate = targetRate,
            IsBuyAlert = isBuyAlert,
            CreatedAt = createdAt
        };
    }

    public bool ShouldTrigger(decimal currentRate)
    {
        decimal roundedCurrentRate = Math.Round(currentRate, RatePrecisionDecimals);
        decimal roundedTargetRate = Math.Round(TargetRate, RatePrecisionDecimals);

        return IsBuyAlert
            ? roundedCurrentRate <= roundedTargetRate
            : roundedCurrentRate >= roundedTargetRate;
    }

    public void MarkTriggered()
    {
        IsTriggered = true;
    }
}
