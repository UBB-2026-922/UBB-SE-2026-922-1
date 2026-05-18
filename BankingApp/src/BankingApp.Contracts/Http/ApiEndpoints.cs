namespace BankingApp.Contracts.Http;

public static class ApiEndpoints
{
    public const string Login = "api/auth/login";
    public const string Register = "api/auth/register";
    public const string ForgotPassword = "api/auth/forgot-password";
    public const string VerifyResetToken = "api/auth/verify-reset-token";
    public const string ResetPassword = "api/auth/reset-password";
    public const string VerifyOtp = "api/auth/verify-otp";
    public const string ResendOtp = "api/auth/resend-otp";

    public const string Dashboard = "api/dashboard/";

    public const string Profile = "api/profile/";
    public const string VerifyPassword = "api/profile/verify-password";
    public const string ChangePassword = "api/profile/password";
    public const string Enable2Fa = "api/profile/2fa/enable";
    public const string Disable2Fa = "api/profile/2fa/disable";
    public const string NotificationPreferences = "api/profile/notifications/preferences";
    public const string Sessions = "api/profile/sessions";

    public const string ForexPreview = "api/forex/preview";
    public const string ForexExecute = "api/forex/execute";
    public const string ForexHistory = "api/forex/history";
    public const string RateAlerts = "api/forex/rate-alerts";

    public const string Billers = "api/billers";
    public const string SavedBillers = "api/billers/saved";

    public const string BillPayAccounts = "api/bill-payment/accounts";
    public const string BillPayFee = "api/bill-payment/fee";
    public const string BillPayRequires2Fa = "api/bill-payment/requires-2fa";
    public const string BillPayPay = "api/bill-payment/pay";
    public const string BillPayHistory = "api/bill-payment/history";

    public const string Beneficiaries = "api/beneficiaries";

    public const string RecurringPayments = "api/recurring_payments";

    public const string TransferHistory = "api/transfers";
    public const string TransferAccounts = "api/transfers/accounts";
    public const string TransferValidateIban = "api/transfers/validate-iban";
    public const string TransferFxPreview = "api/transfers/fx-preview";
    public const string TransferExecute = "api/transfers/execute";

    public const string Cards = "api/cards";
}
