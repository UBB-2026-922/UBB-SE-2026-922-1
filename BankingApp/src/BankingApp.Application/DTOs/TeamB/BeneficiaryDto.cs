// <copyright file="BeneficiaryDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the BeneficiaryDto record.
// </summary>

namespace BankingApp.Application.DTOs.TeamB;

/// <summary>
///     Carries beneficiary data between the application and presentation layers.
/// </summary>
/// <param name="Id">The unique identifier (0 for new entries).</param>
/// <param name="UserId">The owning user's identifier.</param>
/// <param name="Name">The display name of the beneficiary.</param>
/// <param name="Iban">The beneficiary's IBAN.</param>
/// <param name="BankName">The beneficiary's bank name (optional).</param>
/// <param name="TotalAmountSent">The cumulative amount sent to this beneficiary.</param>
/// <param name="TransferCount">The total number of transfers made.</param>
/// <param name="LastTransferDate">The date of the most recent transfer, or null.</param>
public record BeneficiaryDto(
    int Id,
    int UserId,
    string Name,
    string Iban,
    string? BankName,
    decimal TotalAmountSent,
    int TransferCount,
    DateTime? LastTransferDate);
