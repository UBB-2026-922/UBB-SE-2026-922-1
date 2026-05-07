namespace BankingApp.Desktop.Utilities;

/// <summary>
///     Centralises all user-facing message strings shown by ViewModels.
///     Keeps display text out of the call sites and in one place so it is
///     easy to review, update, or replace with a proper localisation layer later.
/// </summary>
internal static class UserMessages
{
    /// <summary>Messages shown in the dashboard view.</summary>
    internal static class Dashboard
    {
        /// <summary>Shown when the server rejects the request as unauthorised (expired session).</summary>
        internal const string SessionExpired = "Your session expired. Please sign in again.";

        /// <summary>Shown when the server cannot find dashboard data for this account.</summary>
        internal const string NotFound = "Dashboard data was not found for this account.";

        /// <summary>Shown for any other load failure.</summary>
        internal const string LoadFailed = "We couldn't load your dashboard. Please try again.";

        /// <summary>Shown when the dashboard response is missing required data.</summary>
        internal const string IncompleteResponse = "The dashboard response was incomplete.";
    }

    /// <summary>Messages shown in the 2FA view.</summary>
    internal static class TwoFactor
    {
        /// <summary>Shown when the user submits a code that is not exactly 6 digits.</summary>
        internal const string InvalidCodeFormat = "Please enter a valid 6-digit code.";

        /// <summary>Shown when the server rejects the submitted code.</summary>
        internal const string IncorrectCode = "The code you entered is incorrect.";
    }

    /// <summary>Messages shown in the registration view.</summary>
    internal static class Register
    {
        /// <summary>Shown when the email address is already associated with an account.</summary>
        internal const string EmailAlreadyExists = "This email is already registered.";

        /// <summary>Shown when the entered email address is not valid.</summary>
        internal const string InvalidEmail = "Please enter a valid email address.";

        /// <summary>Shown when the chosen password does not meet strength requirements.</summary>
        internal const string WeakPassword =
            "Password must be at least 8 characters with uppercase, lowercase, a digit and a special character.";

        /// <summary>Shown when the password and confirmation password do not match.</summary>
        internal const string PasswordMismatch = "Passwords do not match.";

        /// <summary>Shown when one or more required fields are left blank.</summary>
        internal const string AllFieldsRequired = "Please fill in all fields.";
    }

    /// <summary>Messages shown in the profile view.</summary>
    internal static class Profile
    {
        /// <summary>Shown when no phone number has been set for the user.</summary>
        internal const string NoPhoneNumber = "No phone number set";
    }

    /// <summary>Messages shown for security-related operations.</summary>
    internal static class Security
    {
        /// <summary>Shown when the new password does not meet the minimum length requirement.</summary>
        internal const string MinimumLengthRequired = "Minimum 8 characters required.";

        /// <summary>Shown when the new password and confirmation do not match.</summary>
        internal const string PasswordMismatch = "Passwords do not match.";

        /// <summary>Shown when the current password verification fails.</summary>
        internal const string IncorrectPassword = "Current password is incorrect.";

        /// <summary>Shown when an unexpected error occurs during a security operation.</summary>
        internal const string UnexpectedError = "An unexpected error occurred.";
    }

    /// <summary>Messages shown in the forgot-password view.</summary>
    internal static class ForgotPassword
    {
        /// <summary>Shown when the email field is empty on submission.</summary>
        internal const string EmailRequired = "Please enter your email address.";

        /// <summary>Shown when required fields are left empty on the reset form.</summary>
        internal const string AllFieldsRequired = "Please fill in all fields.";

        /// <summary>Shown when the chosen password does not meet the complexity requirements.</summary>
        internal const string PasswordTooWeak =
            "Password must be at least 8 characters with uppercase, lowercase, a digit, and a special character.";

        /// <summary>Shown when the user tries to verify without pasting a recovery code.</summary>
        internal const string CodeRequired = "Please paste the recovery code first.";
    }

    /// <summary>Messages shown in the currency exchange view.</summary>
    internal static class Exchange
    {
        /// <summary>Shown when the source or target currency is not selected.</summary>
        internal const string CurrencyRequired = "Please select both source and target currencies.";

        /// <summary>Shown when the exchange amount is zero or empty.</summary>
        internal const string AmountRequired = "Please enter an amount greater than zero.";

        /// <summary>Shown when the rate preview API call fails.</summary>
        internal const string PreviewFailed = "Could not load the exchange rate preview. Please try again.";

        /// <summary>Shown when the exchange execution API call fails.</summary>
        internal const string ExecuteFailed = "The exchange could not be completed. Please try again.";
    }

    /// <summary>Messages shown in the rate alerts view.</summary>
    internal static class RateAlerts
    {
        /// <summary>Shown when loading alerts from the API fails.</summary>
        internal const string LoadFailed = "Could not load your rate alerts. Please try again.";

        /// <summary>Shown when the base or target currency is not selected.</summary>
        internal const string CurrencyRequired = "Base currency and target currency are required.";

        /// <summary>Shown when the base and target currencies are the same.</summary>
        internal const string CurrenciesMustDiffer = "Base currency and target currency must be different.";

        /// <summary>Shown when the target rate text contains an ambiguous number format.</summary>
        internal const string InvalidNumberFormat = "Invalid number format.";

        /// <summary>Shown when the target rate cannot be parsed or is not positive.</summary>
        internal const string InvalidTargetRate = "Target rate must be a valid number (e.g. 1,2 or 1.2).";

        /// <summary>Shown when creating an alert via the API fails.</summary>
        internal const string CreateFailed = "Could not create the rate alert. Please try again.";

        /// <summary>Shown when deleting an alert via the API fails.</summary>
        internal const string DeleteFailed = "Could not delete the rate alert. Please try again.";
    }

    /// <summary>Messages shown in the transfer history view.</summary>
    internal static class TransferHistory
    {
        /// <summary>Shown when loading the history list from the API fails.</summary>
        internal const string LoadFailed = "Could not load your transfer history. Please try again.";

        /// <summary>Shown when the history list is empty and no transfers have been made.</summary>
        internal const string NoTransfers = "You haven't made any transfers yet.";
    }

    /// <summary>Messages shown in the transfer view.</summary>
    internal static class Transfer
    {
        /// <summary>Shown when the IBAN entered by the user fails validation.</summary>
        internal const string InvalidIban = "Invalid IBAN format.";

        /// <summary>Shown when the transfer amount is zero or negative.</summary>
        internal const string AmountMustBePositive = "The amount must be greater than 0.";

        /// <summary>Shown when the user tries to proceed past the 2FA step without confirming.</summary>
        internal const string TwoFaRequired = "You must confirm the 2FA step.";

        /// <summary>Shown when no source account has been selected before submitting a transfer.</summary>
        internal const string NoAccountSelected = "No account selected.";

        /// <summary>Shown when the account list could not be loaded from the API.</summary>
        internal const string AccountLoadFailed = "Could not load your accounts. Please try again.";

        /// <summary>Shown when the transfer execution fails on the server.</summary>
        internal const string TransferFailed = "Transfer failed. Please try again.";
    }
}
