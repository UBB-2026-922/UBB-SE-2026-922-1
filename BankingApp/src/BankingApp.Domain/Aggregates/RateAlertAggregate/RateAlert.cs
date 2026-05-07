namespace BankingApp.Domain.Aggregates.RateAlertAggregate;

using BankingApp.Domain.Common.Primitives;
using BankingApp.Domain.ValueObjects;
using ErrorOr;

public sealed class RateAlert : AggregateRoot<int>
{
    private const int RatePrecisionDecimals = 2;

    private RateAlert()
    {
    }

    public int UserId { get; private set; }

    public ExchangeRate Target { get; private set; } = default!;

    public bool IsTriggered { get; private set; }

    public bool IsBuyAlert { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static RateAlert Create(
        int userId,
        ExchangeRate target,
        bool isBuyAlert,
        DateTime createdAt)
    {
        return new RateAlert
        {
            UserId = userId,
            Target = target,
            IsBuyAlert = isBuyAlert,
            CreatedAt = createdAt
        };
    }

    public bool ShouldTrigger(decimal currentRate)
    {
        decimal roundedCurrentRate = Math.Round(currentRate, RatePrecisionDecimals);
        decimal roundedTargetRate = Math.Round(Target.Value, RatePrecisionDecimals);

        return IsBuyAlert
            ? roundedCurrentRate <= roundedTargetRate
            : roundedCurrentRate >= roundedTargetRate;
    }

    public void MarkTriggered()
    {
        IsTriggered = true;
    }
}
