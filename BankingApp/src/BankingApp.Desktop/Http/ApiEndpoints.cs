namespace BankingApp.Desktop.Utilities;

/// <summary>Defines the relative API endpoint paths used by the client.</summary>
public static class ApiEndpoints
{
    /// <summary>POST api/auth/login</summary>
    public const string Login = "api/auth/login";
    /// <summary>POST api/auth/register</summary>
    public const string Register = "api/auth/register";
    /// <summary>POST api/auth/forgot-password</summary>
    public const string ForgotPassword = "api/auth/forgot-password";
    /// <summary>POST api/auth/verify-reset-token</summary>
    public const string VerifyResetToken = "api/auth/verify-reset-token";
    /// <summary>POST api/auth/reset-password</summary>
    public const string ResetPassword = "api/auth/reset-password";
    /// <summary>POST api/auth/verify-otp</summary>
    public const string VerifyOtp = "api/auth/verify-otp";
    /// <summary>POST api/auth/resend-otp</summary>
    public const string ResendOtp = "api/auth/resend-otp";
    /// <summary>GET api/dashboard/</summary>
    public const string Dashboard = "api/dashboard/";
    /// <summary>GET/PUT api/profile/</summary>
    public const string Profile = "api/profile/";
    /// <summary>POST api/profile/verify-password</summary>
    public const string VerifyPassword = "api/profile/verify-password";
    /// <summary>PUT api/profile/password</summary>
    public const string ChangePassword = "api/profile/password";
    /// <summary>PUT api/profile/2fa/enable</summary>
    public const string Enable2Fa = "api/profile/2fa/enable";
    /// <summary>PUT api/profile/2fa/disable</summary>
    public const string Disable2Fa = "api/profile/2fa/disable";
    /// <summary>GET/PUT api/profile/notifications/preferences</summary>
    public const string NotificationPreferences = "api/profile/notifications/preferences";
    /// <summary>GET/DELETE api/profile/sessions</summary>
    public const string Sessions = "api/profile/sessions";
    /// <summary>GET api/exchange/preview</summary>
    public const string ExchangePreview = "api/exchange/preview";
    /// <summary>POST api/exchange/execute</summary>
    public const string ExchangeExecute = "api/exchange/execute";
    /// <summary>GET/POST/DELETE api/exchange/rate-alerts</summary>
    public const string RateAlerts = "api/exchange/rate-alerts";
    /// <summary>GET api/billers</summary>
    public const string BillPayBillers = "api/billers";
    /// <summary>GET api/billers (search)</summary>
    public const string BillPayBillersSearch = "api/billers";
    /// <summary>GET api/billers/saved</summary>
    public const string BillPaySavedBillers = "api/billers/saved";
    /// <summary>POST api/billers/saved</summary>
    public const string BillPaySaveBiller = "api/billers/saved";
    /// <summary>GET api/bill-payment/accounts</summary>
    public const string BillPayAccounts = "api/bill-payment/accounts";
    /// <summary>GET api/bill-payment/fee</summary>
    public const string BillPayFee = "api/bill-payment/fee";
    /// <summary>GET api/bill-payment/requires-2fa</summary>
    public const string BillPayRequires2Fa = "api/bill-payment/requires-2fa";
    /// <summary>POST api/bill-payment/pay</summary>
    public const string BillPayPay = "api/bill-payment/pay";
    /// <summary>GET/POST/DELETE api/beneficiaries</summary>
    public const string Beneficiaries = "api/beneficiaries";
    /// <summary>GET/POST api/recurringpayments</summary>
    public const string RecurringPayments = "api/recurringpayments";
    /// <summary>GET api/transfers</summary>
    public const string TransferHistory = "api/transfers";
    /// <summary>GET api/transfers/accounts</summary>
    public const string TransferAccounts = "api/transfers/accounts";
    /// <summary>POST api/transfers/validate-iban</summary>
    public const string TransferValidateIban = "api/transfers/validate-iban";
    /// <summary>GET api/transfers/fx-preview</summary>
    public const string TransferFxPreview = "api/transfers/fx-preview";
    /// <summary>POST api/transfers/execute</summary>
    public const string TransferExecute = "api/transfers/execute";
}
