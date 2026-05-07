// <copyright file="BillPaymentDtos.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains DTOs for the bill payment feature.
// </summary>

namespace BankingApp.Application.DTOs.BillPayment;

/// <summary>
///     Represents a biller in the directory.
/// </summary>
public class BillerDto
{
    /// <summary>
    ///     Gets or sets the biller identifier.
    /// </summary>
    /// <value>The biller identifier.</value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the biller display name.
    /// </summary>
    /// <value>The biller display name.</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the biller category (e.g. Utilities, Internet).
    /// </summary>
    /// <value>The biller category.</value>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the optional logo URL.
    /// </summary>
    /// <value>The logo URL.</value>
    public string? LogoUrl { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the biller is active.
    /// </summary>
    /// <value>Whether the biller is active.</value>
    public bool IsActive { get; set; }
}

/// <summary>
///     Represents a biller saved by the user for quick access.
/// </summary>
public class SavedBillerDto
{
    /// <summary>
    ///     Gets or sets the saved biller identifier.
    /// </summary>
    /// <value>The saved biller identifier.</value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the user identifier who saved this biller.
    /// </summary>
    /// <value>The user identifier.</value>
    public int UserId { get; set; }

    /// <summary>
    ///     Gets or sets the biller identifier.
    /// </summary>
    /// <value>The biller identifier.</value>
    public int BillerId { get; set; }

    /// <summary>
    ///     Gets or sets the user-assigned nickname for this saved biller.
    /// </summary>
    /// <value>The nickname.</value>
    public string? Nickname { get; set; }

    /// <summary>
    ///     Gets or sets the biller name returned by the main billers API.
    /// </summary>
    /// <value>The biller name.</value>
    public string BillerName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the biller category returned by the main billers API.
    /// </summary>
    /// <value>The biller category.</value>
    public string BillerCategory { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the biller logo URL returned by the main billers API.
    /// </summary>
    /// <value>The biller logo URL.</value>
    public string? LogoUrl { get; set; }

    /// <summary>
    ///     Gets or sets the default reference to pre-fill on selection.
    /// </summary>
    /// <value>The default reference.</value>
    public string? DefaultReference { get; set; }

    /// <summary>
    ///     Gets or sets the date the biller was saved.
    /// </summary>
    /// <value>The creation date.</value>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Gets or sets the associated biller details.
    /// </summary>
    /// <value>The biller details.</value>
    public BillerDto? Biller { get; set; }

    /// <summary>
    ///     Gets the display name for the saved biller.
    /// </summary>
    /// <value>The display name.</value>
    public string DisplayName => string.IsNullOrWhiteSpace(Nickname) ? BillerName : Nickname;

    /// <summary>
    ///     Gets the category display text for the saved biller.
    /// </summary>
    /// <value>The category display text.</value>
    public string DisplayCategory => Biller?.Category ?? BillerCategory;

    /// <summary>
    ///     Converts the saved biller response into a biller selection.
    /// </summary>
    /// <returns>The selected biller.</returns>
    public BillerDto ToBiller()
    {
        return Biller ?? new BillerDto
        {
            Id = BillerId,
            Name = BillerName,
            Category = BillerCategory,
            LogoUrl = LogoUrl,
            IsActive = true
        };
    }
}

/// <summary>
///     Represents the request payload for paying a bill.
/// </summary>
public class BillPayRequestDto
{
    /// <summary>
    ///     Gets or sets the source account identifier.
    /// </summary>
    /// <value>The source account identifier.</value>
    public int SourceAccountId { get; set; }

    /// <summary>
    ///     Gets or sets the biller identifier.
    /// </summary>
    /// <value>The biller identifier.</value>
    public int BillerId { get; set; }

    /// <summary>
    ///     Gets or sets the biller reference (account number, contract ID, etc.).
    /// </summary>
    /// <value>The biller reference.</value>
    public string BillerReference { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the payment amount.
    /// </summary>
    /// <value>The payment amount.</value>
    public decimal Amount { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to pay the full outstanding balance.
    /// </summary>
    /// <value>Whether to pay in full.</value>
    public bool IsPayInFull { get; set; }

    /// <summary>
    ///     Gets or sets the optional 2FA token for high-value payments.
    /// </summary>
    /// <value>The 2FA token.</value>
    public string? TwoFaToken { get; set; }
}

/// <summary>
///     Represents the response returned after a successful bill payment.
/// </summary>
public class BillPayResponseDto
{
    /// <summary>
    ///     Gets or sets the payment identifier.
    /// </summary>
    /// <value>The payment identifier.</value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the receipt number.
    /// </summary>
    /// <value>The receipt number.</value>
    public string ReceiptNumber { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the fee charged.
    /// </summary>
    /// <value>The fee amount.</value>
    public decimal Fee { get; set; }

    /// <summary>
    ///     Gets or sets the payment amount.
    /// </summary>
    /// <value>The payment amount.</value>
    public decimal Amount { get; set; }

    /// <summary>
    ///     Gets or sets the payment status.
    /// </summary>
    /// <value>The payment status.</value>
    public string Status { get; set; } = string.Empty;
}

/// <summary>
///     Represents the request to save a biller for future use.
/// </summary>
public class SaveBillerRequestDto
{
    /// <summary>
    ///     Gets or sets the biller identifier to save.
    /// </summary>
    /// <value>The biller identifier.</value>
    public int BillerId { get; set; }

    /// <summary>
    ///     Gets or sets the user-assigned nickname.
    /// </summary>
    /// <value>The nickname.</value>
    public string? Nickname { get; set; }

    /// <summary>
    ///     Gets or sets the default reference for this biller.
    /// </summary>
    /// <value>The default reference.</value>
    public string? DefaultReference { get; set; }
}

/// <summary>
///     Represents the fee calculation response.
/// </summary>
public class FeeResponseDto
{
    /// <summary>
    ///     Gets or sets the calculated fee.
    /// </summary>
    /// <value>The fee amount.</value>
    public decimal Fee { get; set; }
}

/// <summary>
///     Represents the 2FA requirement check response.
/// </summary>
public class Requires2FaResponseDto
{
    /// <summary>
    ///     Gets or sets a value indicating whether 2FA is required.
    /// </summary>
    /// <value>Whether 2FA is required.</value>
    public bool Required { get; set; }
}

/// <summary>
///     Represents a user account for bill payment source selection.
/// </summary>
public class AccountDto
{
    /// <summary>
    ///     Gets or sets the account identifier.
    /// </summary>
    /// <value>The account identifier.</value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the account IBAN.
    /// </summary>
    /// <value>The IBAN.</value>
    public string Iban { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the account currency.
    /// </summary>
    /// <value>The currency code.</value>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the account balance.
    /// </summary>
    /// <value>The account balance.</value>
    public decimal Balance { get; set; }

    /// <summary>
    ///     Gets or sets the account display name.
    /// </summary>
    /// <value>The account name.</value>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the account status.
    /// </summary>
    /// <value>The account status.</value>
    public string Status { get; set; } = string.Empty;
}