// <copyright file="TransferErrors.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the TransferErrors class.
// </summary>

using ErrorOr;

namespace BankingApp.Domain.Errors;

/// <summary>
///     Canonical error definitions for the transfer flow.
/// </summary>
public static class TransferErrors
{
    /// <summary>The recipient IBAN failed structural validation.</summary>
    public static readonly Error InvalidIban =
        Error.Validation("transfer.invalid_iban", "The recipient IBAN is invalid.");

    /// <summary>The transfer amount must be greater than zero.</summary>
    public static readonly Error InvalidAmount =
        Error.Validation("transfer.invalid_amount", "Transfer amount must be greater than zero.");

    /// <summary>The currency code must be exactly 3 characters.</summary>
    public static readonly Error InvalidCurrency =
        Error.Validation("transfer.invalid_currency", "Currency code must be exactly 3 characters.");

    /// <summary>The source account does not exist or does not belong to the user.</summary>
    public static readonly Error AccountNotFound =
        Error.NotFound("transfer.account_not_found", "Source account was not found.");

    /// <summary>The source account is not active (suspended or closed).</summary>
    public static readonly Error AccountNotActive =
        Error.Forbidden("transfer.account_not_active", "Source account is not active.");

    /// <summary>The source account has insufficient funds to cover the transfer and fee.</summary>
    public static readonly Error InsufficientFunds =
        Error.Forbidden("transfer.insufficient_funds", "Insufficient funds in the source account.");

    /// <summary>A 2FA token is required for transfers of 1000 or more but was not provided.</summary>
    public static readonly Error TwoFaRequired =
        Error.Forbidden("transfer.2fa_required", "A 2FA token is required for transfers of 1000 or more.");

    /// <summary>The provided 2FA token is invalid or expired.</summary>
    public static readonly Error InvalidTwoFaToken =
        Error.Unauthorized("transfer.invalid_2fa_token", "The provided 2FA token is invalid or expired.");

    /// <summary>The transfer record could not be persisted.</summary>
    public static readonly Error PersistenceFailed =
        Error.Failure("transfer.persistence_failed", "Failed to save the transfer record.");

    /// <summary>The transaction log entry could not be persisted.</summary>
    public static readonly Error TransactionLogFailed =
        Error.Failure("transfer.transaction_log_failed", "Failed to log the transaction record.");

    /// <summary>The account debit operation failed.</summary>
    public static readonly Error DebitFailed =
        Error.Failure("transfer.debit_failed", "Failed to debit the source account.");
}