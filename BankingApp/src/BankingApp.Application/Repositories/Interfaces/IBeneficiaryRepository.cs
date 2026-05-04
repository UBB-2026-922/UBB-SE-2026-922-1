// <copyright file="IBeneficiaryRepository.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the IBeneficiaryRepository interface.
// </summary>

using BankingApp.Domain.Entities;
using ErrorOr;

namespace BankingApp.Application.Repositories.Interfaces;

/// <summary>
///     Defines persistence operations for the <see cref="Beneficiary" /> entity.
/// </summary>
public interface IBeneficiaryRepository
{
    /// <summary>Retrieves a beneficiary by its unique identifier.</summary>
    /// <param name="id">The beneficiary identifier.</param>
    /// <returns>The matching <see cref="Beneficiary" />, or an error when not found.</returns>
    ErrorOr<Beneficiary> GetById(int id);

    /// <summary>Retrieves all beneficiaries belonging to the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of beneficiaries, or an error.</returns>
    ErrorOr<List<Beneficiary>> GetByUserId(int userId);

    /// <summary>Persists a new beneficiary record.</summary>
    /// <param name="beneficiary">The beneficiary to create.</param>
    /// <returns>The created beneficiary with its assigned identifier, or an error.</returns>
    ErrorOr<Beneficiary> Create(Beneficiary beneficiary);

    /// <summary>Updates a beneficiary's mutable fields (name, bank name, statistics).</summary>
    /// <param name="beneficiary">The beneficiary with updated values.</param>
    /// <returns>The updated beneficiary, or an error.</returns>
    ErrorOr<Beneficiary> Update(Beneficiary beneficiary);

    /// <summary>Removes a beneficiary from the user's address book.</summary>
    /// <param name="id">The identifier of the beneficiary to remove.</param>
    /// <returns>Success, or an error when the record is not found.</returns>
    ErrorOr<Success> Delete(int id);
}
