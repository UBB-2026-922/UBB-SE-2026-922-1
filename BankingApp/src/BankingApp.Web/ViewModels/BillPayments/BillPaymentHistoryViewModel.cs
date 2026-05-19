namespace BankingApp.Web.ViewModels.BillPayments;

/// <summary>
///     View model for the bill-payment history page (GET /BillPayments/History).
/// </summary>
public class BillPaymentHistoryViewModel
{
    /// <summary>Gets or sets the list of past bill payments, newest first.</summary>
    public List<BillPaymentRowViewModel> Payments { get; set; } = [];

    /// <summary>Gets a value indicating whether there are any payments to display.</summary>
    public bool HasPayments => Payments.Count > 0;
}
