// <copyright file="CreateBeneficiaryRequest.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the CreateBeneficiaryRequest class.
// </summary>

namespace BankingApp.Application.DataTransferObjects.Beneficiary;

/// <summary>
///     Represents the data required to create a beneficiary.
/// </summary>
public class CreateBeneficiaryRequest
{
    /// <summary>
    ///     Gets or sets the beneficiary name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the beneficiary IBAN.
    /// </summary>
    public string Iban { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the beneficiary bank name.
    /// </summary>
    public string? BankName { get; set; }
}