namespace BankingApp.Web.ViewModels.RecurringPayments;

public sealed class RecurringPaymentListViewModel
{
    public List<RecurringPaymentRowViewModel> Payments { get; set; } = [];

    public bool HasPayments => Payments.Count > 0;
}
