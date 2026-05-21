namespace BankingApp.Web.ViewModels.Forex;

/// <summary>
///     View model for the forex transaction history page (GET /Forex/History).
/// </summary>
public class ForexHistoryViewModel
{
    /// <summary>Gets or sets the list of past forex transactions, newest first.</summary>
    public List<ForexHistoryRowViewModel> Transactions { get; set; } = [];

    /// <summary>Gets a value indicating whether there are any transactions to display.</summary>
    public bool HasTransactions => Transactions.Count > 0;
}
