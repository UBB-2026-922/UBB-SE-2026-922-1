// <copyright file="BillPaymentResponseDto.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the BillPaymentResponseDto class.
// </summary>

using BankingApp.Domain.Enums;

namespace BankingApp.Application.DTOs.TeamB;

/// <summary>
///     Carries the result data of a bill payment operation.
/// </summary>
public class BillPaymentResponseDto
{
    /// <summary>Gets or sets the unique identifier of the payment.</summary>
    /// <value>Gets or sets the current value.</value>
    public int Id { get; set; }

    /// <summary>Gets or sets the display name of the biller that was paid.</summary>
    /// <value>Gets or sets the current value.</value>
    public string BillerName { get; set; } = string.Empty;

    /// <summary>Gets or sets the biller-specific reference used for this payment.</summary>
    /// <value>Gets or sets the current value.</value>
    public string BillerReference { get; set; } = string.Empty;

    /// <summary>Gets or sets the amount paid.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the processing fee charged.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal Fee { get; set; }

    /// <summary>Gets or sets the unique receipt number issued upon confirmation.</summary>
    /// <value>Gets or sets the current value.</value>
    public string ReceiptNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the current processing status.</summary>
    /// <value>Gets or sets the current value.</value>
    public BillPaymentStatus Status { get; set; }

    /// <summary>Gets or sets the timestamp when the payment was created.</summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime CreatedAt { get; set; }
}