// <copyright file="TransferFxPreviewResponse.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the TransferFxPreviewResponse class.
// </summary>

namespace BankingApp.Application.DTOs.Transfer;

/// <summary>
///     Represents the FX preview for a transfer.
/// </summary>
public class TransferFxPreviewResponse
{
    /// <summary>Gets or sets the exchange rate.</summary>
    public decimal ExchangeRate { get; set; }

    /// <summary>Gets or sets the converted amount.</summary>
    public decimal ConvertedAmount { get; set; }
}