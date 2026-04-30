// <copyright file="BeneficiaryDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the BeneficiaryDto class.
// </summary>

namespace BankingApp.Application.DTOs.TeamB;

/// <summary>
///     Carries beneficiary data between the application and presentation layers.
/// </summary>
public class BeneficiaryDto
{
    /// <summary>Gets or sets the unique identifier (0 for new entries).</summary>
    /// <value>Gets or sets the current value.</value>
    public int Id { get; set; }

    /// <summary>Gets or sets the owning user's identifier.</summary>
    /// <value>Gets or sets the current value.</value>
    public int UserId { get; set; }

    /// <summary>Gets or sets the display name of the beneficiary.</summary>
    /// <value>Gets or sets the current value.</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the beneficiary's IBAN.</summary>
    /// <value>Gets or sets the current value.</value>
    public string Iban { get; set; } = string.Empty;

    /// <summary>Gets or sets the beneficiary's bank name, or <see langword="null" /> if not known.</summary>
    /// <value>Gets or sets the current value.</value>
    public string? BankName { get; set; }

    /// <summary>Gets or sets the cumulative amount sent to this beneficiary.</summary>
    /// <value>Gets or sets the current value.</value>
    public decimal TotalAmountSent { get; set; }

    /// <summary>Gets or sets the total number of transfers made.</summary>
    /// <value>Gets or sets the current value.</value>
    public int TransferCount { get; set; }

    /// <summary>Gets or sets the date of the most recent transfer, or <see langword="null" /> if none.</summary>
    /// <value>Gets or sets the current value.</value>
    public DateTime? LastTransferDate { get; set; }
}
