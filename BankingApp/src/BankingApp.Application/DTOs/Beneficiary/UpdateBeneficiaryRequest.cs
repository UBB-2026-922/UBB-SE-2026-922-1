// <copyright file="UpdateBeneficiaryRequest.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the UpdateBeneficiaryRequest class.
// </summary>

namespace BankingApp.Application.DataTransferObjects.Beneficiary;

/// <summary>
///     Represents the data required to update a beneficiary.
/// </summary>
public class UpdateBeneficiaryRequest
{
    /// <summary>
    ///     Gets or sets the beneficiary identifier.
    /// </summary>
    public int Id { get; set; }

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
