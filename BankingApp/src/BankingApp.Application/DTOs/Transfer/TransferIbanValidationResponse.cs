// <copyright file="TransferIbanValidationResponse.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the TransferIbanValidationResponse class.
// </summary>

namespace BankingApp.Application.DTOs.Transfer;

/// <summary>
///     Represents the result of validating a transfer recipient IBAN.
/// </summary>
public class TransferIbanValidationResponse
{
    /// <summary>Gets or sets a value indicating whether the IBAN is valid.</summary>
    public bool IsValid { get; set; }

    /// <summary>Gets or sets the inferred bank name.</summary>
    public string BankName { get; set; } = string.Empty;
}