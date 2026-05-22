namespace BankingApp.Web.ViewModels.RateAlerts;

/// <summary>
///     View model for the rate alerts list page (GET /RateAlerts).
/// </summary>
public class RateAlertListViewModel
{
    /// <summary>Gets or sets the list of active rate alerts.</summary>
    public List<RateAlertRowViewModel> Alerts { get; set; } = [];

    /// <summary>Gets a value indicating whether there are any alerts to display.</summary>
    public bool HasAlerts => Alerts.Count > 0;
}
