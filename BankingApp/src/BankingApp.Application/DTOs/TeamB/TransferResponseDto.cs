// <copyright file="TransferResponseDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the TransferResponseDto class.
// </summary>

using BankingApp.Domain.Enums;

namespace BankingApp.Application.DTOs.TeamB;

/// <summary>
///     Carries the result data of a bank transfer operation.
/// </summary>
public class TransferResponseDto
{
    /// <summary>Gets or sets the unique identifier of the transfer.</summary>
    /// <value>Gets or sets the current value.</value>
    public int Id { get; set; }

    /// <summary>Gets or sets the full name of the recipient.</summary>
    /// <value>Gets or sets the current value.</value>
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>Gets or sets the recipient's IBAN.</summary>
    /// <value>Gets or sets the current value.</value>
    public string RecipientIban { get; set; } = string.Empty;

    /// <summary>Gets or sets the transferred amount.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the ISO 4217 currency code.</summary>
    /// <value>Gets or sets the current value.</value>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Gets or sets the fee charged for the transfer.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal Fee { get; set; }

    /// <summary>Gets or sets the current processing status.</summary>
    /// <value>Gets or sets the current value.</value>
    public TransferStatus Status { get; set; }

    /// <summary>Gets or sets the estimated arrival date at the recipient bank, or <see langword="null" /> if not yet determined.</summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime? EstimatedArrival { get; set; }

    /// <summary>Gets or sets the timestamp when the transfer was created.</summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime CreatedAt { get; set; }
}
