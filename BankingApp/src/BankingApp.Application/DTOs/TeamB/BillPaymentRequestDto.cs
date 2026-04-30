// <copyright file="BillPaymentRequestDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the BillPaymentRequestDto class.
// </summary>

namespace BankingApp.Application.DTOs.TeamB;

/// <summary>
///     Carries the input data required to process a one-off bill payment.
/// </summary>
public class BillPaymentRequestDto
{
    /// <summary>Gets or sets the identifier of the paying user.</summary>
    /// <value>Gets or sets the current value.</value>
    public int UserId { get; set; }

    /// <summary>Gets or sets the identifier of the account to debit.</summary>
    /// <value>Gets or sets the current value.</value>
    public int SourceAccountId { get; set; }

    /// <summary>Gets or sets the identifier of the target biller.</summary>
    /// <value>Gets or sets the current value.</value>
    public int BillerId { get; set; }

    /// <summary>Gets or sets the biller-specific reference (e.g., invoice or contract number).</summary>
    /// <value>Gets or sets the current value.</value>
    public string BillerReference { get; set; } = string.Empty;

    /// <summary>Gets or sets the amount to pay.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal Amount { get; set; }
}
