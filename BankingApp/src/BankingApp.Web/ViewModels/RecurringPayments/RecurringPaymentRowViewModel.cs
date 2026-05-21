namespace BankingApp.Web.ViewModels.RecurringPayments;

using Domain.Enums;

public sealed class RecurringPaymentRowViewModel
{
    public int Id { get; set; }

    public int BillerId { get; set; }

    public int SourceAccountId { get; set; }

    public decimal Amount { get; set; }

    public bool IsPayInFull { get; set; }

    public RecurringFrequency Frequency { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime NextExecutionDate { get; set; }

    public RecurringPaymentStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public string StatusBadgeClass => Status switch
    {
        RecurringPaymentStatus.Active    => "bg-success",
        RecurringPaymentStatus.Paused    => "bg-warning text-dark",
        RecurringPaymentStatus.Cancelled => "bg-secondary",
        _                                => "bg-secondary"
    };

    public bool CanPause => Status is RecurringPaymentStatus.Active;

    public bool CanResume => Status is RecurringPaymentStatus.Paused;

    public bool CanCancel => Status is not RecurringPaymentStatus.Cancelled;
}
