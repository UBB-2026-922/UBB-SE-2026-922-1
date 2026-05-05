// <copyright file="ApiEndpoints.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the ApiEndpoints class.
// </summary>

namespace BankingApp.Desktop.Utilities;

/// <summary>
///     Defines the relative API endpoint paths used by the client.
/// </summary>
public static class ApiEndpoints
{
    /// <summary>POST api/auth/login — credential login.</summary>
    public const string Login = "api/auth/login";

    /// <summary>POST api/auth/oauth-login — OAuth provider login or registration.</summary>
    public const string OAuthLogin = "api/auth/oauth-login";

    /// <summary>POST api/auth/register — new account registration.</summary>
    public const string Register = "api/auth/register";

    /// <summary>POST api/auth/forgot-password — request a password-reset code.</summary>
    public const string ForgotPassword = "api/auth/forgot-password";

    /// <summary>POST api/auth/verify-reset-token — validate a password-reset token.</summary>
    public const string VerifyResetToken = "api/auth/verify-reset-token";

    /// <summary>POST api/auth/reset-password — apply a new password via reset token.</summary>
    public const string ResetPassword = "api/auth/reset-password";

    /// <summary>POST api/auth/verify-otp — submit a OTP for 2FA.</summary>
    public const string VerifyOtp = "api/auth/verify-otp";

    /// <summary>POST api/auth/resend-otp — request a new OTP code.</summary>
    public const string ResendOtp = "api/auth/resend-otp";

    /// <summary>GET api/dashboard/ — load the authenticated user's dashboard data.</summary>
    public const string Dashboard = "api/dashboard/";

    /// <summary>GET/PUT api/profile/ — load or update the user's profile.</summary>
    public const string Profile = "api/profile/";

    /// <summary>POST api/profile/verify-password — verify the user's current password.</summary>
    public const string VerifyPassword = "api/profile/verify-password";

    /// <summary>PUT api/profile/password — change the user's password.</summary>
    public const string ChangePassword = "api/profile/password";

    /// <summary>PUT api/profile/2fa/enable — enable 2FA.</summary>
    public const string Enable2Fa = "api/profile/2fa/enable";

    /// <summary>PUT api/profile/2fa/disable — disable 2FA.</summary>
    public const string Disable2Fa = "api/profile/2fa/disable";

    /// <summary>GET api/profile/oauth-links — list linked OAuth providers.</summary>
    public const string OAuthLinks = "api/profile/oauth-links";

    /// <summary>POST api/profile/oauth/link — link a new OAuth provider.</summary>
    public const string LinkOAuth = "api/profile/oauth/link";

    /// <summary>DELETE api/profile/oauth/{provider} — unlink an OAuth provider.</summary>
    public const string UnlinkOAuth = "api/profile/oauth";

    /// <summary>GET/PUT api/profile/notifications/preferences — load or update notification preferences.</summary>
    public const string NotificationPreferences = "api/profile/notifications/preferences";

    /// <summary>GET api/profile/sessions — list active sessions. DELETE api/profile/sessions/{id} — revoke a session.</summary>
    public const string Sessions = "api/profile/sessions";

    /// <summary>GET api/exchange/preview get a live rate preview for a currency pair and amount.</summary>
    public const string ExchangePreview = "api/exchange/preview";

    /// <summary>POST api/exchange/execute executes a currency exchange between two accounts.</summary>
    public const string ExchangeExecute = "api/exchange/execute";

    /// <summary>GET/POST/DELETE api/exchange/rate-alerts manage rate alerts.</summary>
    public const string RateAlerts = "api/exchange/rate-alerts";

    /// <summary>GET api/billers - list active billers, optionally filtered by category.</summary>
    public const string BillPayBillers = "api/billers";

    /// <summary>GET api/billers - search billers by name and/or category.</summary>
    public const string BillPayBillersSearch = "api/billers";

    /// <summary>GET api/billers/saved - list saved billers for the current user.</summary>
    public const string BillPaySavedBillers = "api/billers/saved";

    /// <summary>POST api/billers/saved - save a biller for future use.</summary>
    public const string BillPaySaveBiller = "api/billers/saved";

    /// <summary>GET api/bill-payment/accounts — list available accounts for bill payment.</summary>
    public const string BillPayAccounts = "api/bill-payment/accounts";

    /// <summary>GET api/bill-payment/fee — calculate fee for a given amount.</summary>
    public const string BillPayFee = "api/bill-payment/fee";

    /// <summary>GET api/bill-payment/requires-2fa — check if 2FA is required for a given amount.</summary>
    public const string BillPayRequires2Fa = "api/bill-payment/requires-2fa";

    /// <summary>POST api/bill-payment/pay — process a bill payment.</summary>
    public const string BillPayPay = "api/bill-payment/pay";

    /// <summary>GET/POST/DELETE api/beneficiaries — manage saved beneficiaries for a user.</summary>
    public const string Beneficiaries = "api/beneficiaries";

    /// <summary>GET/POST api/recurringpayments — list and create recurring payment schedules.</summary>
    public const string RecurringPayments = "api/recurringpayments";

    /// <summary>GET api/transfers/accounts — list the authenticated user's accounts for transfer selection.</summary>
    public const string TransferAccounts = "api/transfers/accounts";

    /// <summary>POST api/transfers/validate-iban — validate an IBAN and return the inferred bank name.</summary>
    public const string TransferValidateIban = "api/transfers/validate-iban";

    /// <summary>GET api/transfers/fx-preview — get an exchange-rate preview for a cross-currency transfer.</summary>
    public const string TransferFxPreview = "api/transfers/fx-preview";

    /// <summary>POST api/transfers/execute — submit a transfer for processing.</summary>
    public const string TransferExecute = "api/transfers/execute";
}
