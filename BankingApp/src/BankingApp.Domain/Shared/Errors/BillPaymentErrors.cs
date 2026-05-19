namespace BankingApp.Domain.Common.Errors;

using ErrorOr;

public static class BillPaymentErrors
{
    public static readonly Error NotFound =
        Error.NotFound("billpayment.not_found", "Bill payment was not found.");

    public static readonly Error InvalidAmount =
        Error.Validation("billpayment.invalid_amount", "Bill payment amount must be greater than zero.");

    public static readonly Error AccountNotActive =
        Error.Forbidden("billpayment.account_not_active", "Source account is not active.");

    public static readonly Error InsufficientFunds =
        Error.Forbidden("billpayment.insufficient_funds", "Insufficient funds in the source account.");

    public static readonly Error InvalidReference =
        Error.Validation("billpayment.invalid_reference", "Biller reference is required.");

    public static readonly Error InvalidFee =
        Error.Validation("billpayment.invalid_fee", "Bill payment fee cannot be negative.");

    public static readonly Error TwoFaRequired =
        Error.Forbidden("billpayment.2fa_required", "A 2FA token is required for this bill payment.");

    public static readonly Error InvalidTwoFaToken =
        Error.Unauthorized("billpayment.invalid_2fa_token", "The provided 2FA token is invalid or expired.");
}
