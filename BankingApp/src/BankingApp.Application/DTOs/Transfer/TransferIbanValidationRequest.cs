// <copyright file="TransferIbanValidationRequest.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the TransferIbanValidationRequest class.
// </summary>

namespace BankingApp.Application.DTOs.Transfer;

/// <summary>
///     Represents the request payload used to validate a recipient IBAN.
/// </summary>
public class TransferIbanValidationRequest
{
    /// <summary>Gets or sets the IBAN to validate.</summary>
    public string Iban { get; set; } = string.Empty;
}
