// <copyright file="BeneficiaryDataTransferObject.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the BeneficiaryDataTransferObject class.
// </summary>

namespace BankingApp.Application.DataTransferObjects.Beneficiary;

/// <summary>
///     Represents beneficiary data returned by the API.
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
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the beneficiary IBAN.
    /// </summary>
    public string Iban { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the beneficiary bank name.
    /// </summary>
    public string? BankName { get; set; }

    /// <summary>
    ///     Gets or sets the date of the last transfer to this beneficiary.
    /// </summary>
    public DateTime? LastTransferDate { get; set; }

    /// <summary>
    ///     Gets or sets the total amount sent to this beneficiary.
    /// </summary>
    public decimal TotalAmountSent { get; set; }

    /// <summary>
    ///     Gets or sets the number of transfers made to this beneficiary.
    /// </summary>
    public int TransferCount { get; set; }

    /// <summary>
    ///     Gets or sets the creation date of the beneficiary.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}