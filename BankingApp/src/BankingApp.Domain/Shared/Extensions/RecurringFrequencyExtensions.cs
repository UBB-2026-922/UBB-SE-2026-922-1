namespace BankingApp.Domain.Common.Extensions;

using Enums;

public static class RecurringFrequencyExtensions
{
    public static DateTime AdvanceFrom(this RecurringFrequency frequency, DateTime from)
    {
        return frequency switch
        {
            RecurringFrequency.Daily => from.AddDays(1),
            RecurringFrequency.Weekly => from.AddDays(7),
            RecurringFrequency.BiWeekly => from.AddDays(14),
            RecurringFrequency.Monthly => from.AddMonths(1),
            RecurringFrequency.Quarterly => from.AddMonths(3),
            RecurringFrequency.Yearly => from.AddYears(1),
            _ => from
        };
    }
}
