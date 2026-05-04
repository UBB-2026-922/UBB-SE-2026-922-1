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
///     Defines repository operations for managing beneficiaries.
/// </summary>
public interface IBeneficiaryRepository
{
    /// <summary>
    ///     Finds a beneficiary by its identifier.
    /// </summary>
    /// <param name="beneficiaryId">The beneficiary identifier.</param>
    /// <returns>
    ///     The beneficiary when found, or a not-found error otherwise.
    /// </returns>
    ErrorOr<Beneficiary> FindById(int beneficiaryId);

    /// <summary>
    ///     Returns all beneficiaries saved by a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>
    ///     The user's beneficiaries, or an error if retrieval fails.
    /// </returns>
    ErrorOr<List<Beneficiary>> FindByUserId(int userId);

    /// <summary>
    ///     Checks whether a beneficiary with the given IBAN already exists for the user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="iban">The beneficiary IBAN.</param>
    /// <returns>
    ///     <see langword="true" /> if one exists; otherwise <see langword="false" />.
    /// </returns>
    ErrorOr<bool> ExistsByUserIdAndIban(int userId, string iban);

    /// <summary>
    ///     Creates a new beneficiary.
    /// </summary>
    /// <param name="beneficiary">The beneficiary to create.</param>
    /// <returns>
    ///     The created beneficiary.
    /// </returns>
    ErrorOr<Beneficiary> Create(Beneficiary beneficiary);

    /// <summary>
    ///     Updates an existing beneficiary.
    /// </summary>
    /// <param name="beneficiary">The beneficiary to update.</param>
    /// <returns>
    ///     <see cref="Result.Success" /> on success, or an error otherwise.
    /// </returns>
    ErrorOr<Success> Update(Beneficiary beneficiary);

    /// <summary>
    ///     Deletes a beneficiary by its identifier.
    /// </summary>
    /// <param name="beneficiaryId">The beneficiary identifier.</param>
    /// <returns>
    ///     <see cref="Result.Success" /> on success, or an error otherwise.
    /// </returns>
    ErrorOr<Success> Delete(int beneficiaryId);
}
