// <copyright file="TransferRequestDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the TransferRequestDto class.
// </summary>

namespace BankingApp.Application.DTOs.TeamB;

/// <summary>
///     Carries the input data required to initiate a bank transfer.
/// </summary>
public class TransferRequestDto
{
    /// <summary>Gets or sets the identifier of the user initiating the transfer.</summary>
    /// <value>Gets or sets the current value.</value>
    public int UserId { get; set; }

    /// <summary>Gets or sets the identifier of the account to debit.</summary>
    /// <value>Gets or sets the current value.</value>
    public int SourceAccountId { get; set; }

    /// <summary>Gets or sets the full name of the recipient.</summary>
    /// <value>Gets or sets the current value.</value>
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>Gets or sets the recipient's IBAN.</summary>
    /// <value>Gets or sets the current value.</value>
    public string RecipientIban { get; set; } = string.Empty;

    /// <summary>Gets or sets the recipient's bank name, or <see langword="null" /> if not known.</summary>
    /// <value>Gets or sets the current value.</value>
    public string? RecipientBankName { get; set; }

    /// <summary>Gets or sets the amount to transfer.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the ISO 4217 currency code.</summary>
    /// <value>Gets or sets the current value.</value>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional free-text payment reference.</summary>
    /// <value>Gets or sets the current value.</value>
    public string? Reference { get; set; }
}
