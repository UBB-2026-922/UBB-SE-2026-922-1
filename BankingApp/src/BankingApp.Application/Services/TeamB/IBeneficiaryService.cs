// <copyright file="IBeneficiaryService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the IBeneficiaryService interface.
// </summary>

using BankingApp.Application.DTOs.TeamB;
using ErrorOr;

namespace BankingApp.Application.Services.TeamB;

/// <summary>
///     Defines application-level operations for beneficiary address-book management.
/// </summary>
public interface IBeneficiaryService
{
    /// <summary>Retrieves all beneficiaries for the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of beneficiary DTOs, or an error.</returns>
    ErrorOr<List<BeneficiaryDto>> GetBeneficiaries(int userId);

    /// <summary>Adds a new beneficiary to the user's address book.</summary>
    /// <param name="dto">The beneficiary data.</param>
    /// <returns>The created beneficiary DTO, or an error.</returns>
    ErrorOr<BeneficiaryDto> AddBeneficiary(BeneficiaryDto dto);

    /// <summary>Removes a beneficiary from the user's address book.</summary>
    /// <param name="id">The beneficiary identifier.</param>
    /// <returns>Success, or an error when not found.</returns>
    ErrorOr<Success> RemoveBeneficiary(int id);
}
