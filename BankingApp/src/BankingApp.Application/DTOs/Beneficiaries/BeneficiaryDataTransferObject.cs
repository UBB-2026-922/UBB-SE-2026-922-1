// <copyright file="BeneficiaryDataTransferObject.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the BeneficiaryDataTransferObject class.
// </summary>

namespace BankingApp.Application.DTOs.Beneficiaries;

/// <summary>
///     Represents a beneficiary saved by a user for future transfers.
/// </summary>
public class BeneficiaryDataTransferObject
{
    /// <summary>
    ///     Gets or sets the beneficiary identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the beneficiary name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the beneficiary IBAN.
    /// </summary>
    public string? Iban { get; set; }

    /// <summary>
    ///     Gets or sets the beneficiary bank name.
    /// </summary>
    public string? BankName { get; set; }
}