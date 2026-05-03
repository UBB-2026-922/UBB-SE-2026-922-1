// <copyright file="BeneficiaryDataTransferObject.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the BeneficiaryDataTransferObject class.
// </summary>

namespace BankingApp.Application.DataTransferObjects.Beneficiaries;

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
    public string? IBAN { get; set; }

    /// <summary>
    ///     Gets or sets the beneficiary bank name.
    /// </summary>
    public string? BankName { get; set; }
}
