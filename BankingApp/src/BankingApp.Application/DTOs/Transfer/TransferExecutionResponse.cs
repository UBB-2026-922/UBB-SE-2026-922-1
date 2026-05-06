// <copyright file="TransferExecutionResponse.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the TransferExecutionResponse class.
// </summary>

namespace BankingApp.Application.DTOs.Transfer;

/// <summary>
///     Represents the compact transfer execution response returned to the desktop wizard.
/// </summary>
public class TransferExecutionResponse
{
    /// <summary>Gets or sets the transaction reference.</summary>
    public string TransactionRef { get; set; } = string.Empty;
}
