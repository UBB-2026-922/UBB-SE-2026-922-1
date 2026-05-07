namespace BankingApp.Application.Features.Beneficiaries.Services;

using Domain.Aggregates.BeneficiaryAggregate;
using ErrorOr;
using Domain.Entities;

/// <summary>
///     Defines business operations for managing beneficiaries.
/// </summary>
public interface IBeneficiaryService
{
    /// <summary>
    ///     Gets all beneficiaries saved by the specified user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>
    ///     The user's beneficiaries, or an error otherwise.
    /// </returns>
    public ErrorOr<List<Beneficiary>> GetByUserId(int userId);

    /// <summary>
    ///     Gets a beneficiary by its identifier.
    /// </summary>
    /// <param name="beneficiaryId">The beneficiary identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns>
    ///     The beneficiary when found, or an error otherwise.
    /// </returns>
    public ErrorOr<Beneficiary> GetById(int beneficiaryId, int userId);

    /// <summary>
    ///     Creates a new beneficiary for the specified user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="name">The beneficiary name.</param>
    /// <param name="iban">The beneficiary IBAN.</param>
    /// <param name="bankName">The beneficiary bank name.</param>
    /// <returns>
    ///     The created beneficiary, or an error otherwise.
    /// </returns>
    public ErrorOr<Beneficiary> Create(int userId, string name, string iban, string? bankName);

    /// <summary>
    ///     Updates an existing beneficiary.
    /// </summary>
    /// <param name="beneficiary">The beneficiary to update.</param>
    /// <returns>
    ///     A success result when the update succeeds, or an error otherwise.
    /// </returns>
    public ErrorOr<Success> Update(Beneficiary beneficiary);

    /// <summary>
    ///     Deletes a beneficiary by its identifier.
    /// </summary>
    /// <param name="beneficiaryId">The beneficiary identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns>
    ///     A success result when the deletion succeeds, or an error otherwise.
    /// </returns>
    public ErrorOr<Success> Delete(int beneficiaryId, int userId);

    /// <summary>
    ///     Validates whether the supplied IBAN has an acceptable format.
    /// </summary>
    /// <param name="iban">The IBAN to validate.</param>
    /// <returns>
    ///     True if the IBAN is valid; otherwise false.
    /// </returns>
    public bool ValidateIban(string iban);
}
