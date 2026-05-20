namespace BankingApp.Web.ViewModels.Profile;

using Domain.Enums;

public sealed class NotificationsViewModel
{
    public IList<NotificationPreferenceRowViewModel> Preferences { get; set; } = [];
}

public sealed class NotificationPreferenceRowViewModel
{
    public NotificationType Category { get; set; }

    public string CategoryDisplayName { get; set; } = string.Empty;

    public bool PushEnabled { get; set; }

    public bool EmailEnabled { get; set; }

    public bool SmsEnabled { get; set; }

    public decimal? MinAmountThreshold { get; set; }

    public string CategoryIcon => Category switch
    {
        NotificationType.Payment           => "bi-credit-card",
        NotificationType.InboundTransfer   => "bi-arrow-down-circle",
        NotificationType.OutboundTransfer  => "bi-arrow-up-circle",
        NotificationType.LowBalance        => "bi-exclamation-triangle",
        NotificationType.DuePayment        => "bi-calendar-check",
        NotificationType.SuspiciousActivity => "bi-shield-exclamation",
        _                                   => "bi-bell"
    };
}
