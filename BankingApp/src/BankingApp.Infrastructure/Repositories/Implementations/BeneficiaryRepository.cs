// <copyright file="BeneficiaryRepository.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the dummy BeneficiaryRepository skeleton for Team B integration.
// </summary>

using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.DataAccess;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Infrastructure.Repositories.Implementations;

/// <summary>
///     Provides repository operations for managing beneficiaries.
/// </summary>
public class BeneficiaryRepository : IBeneficiaryRepository
{
    private readonly AppDatabaseContext _databaseContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BeneficiaryRepository" /> class.
    /// </summary>
    /// <param name="databaseContext">The database context.</param>
    public BeneficiaryRepository(AppDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    /// <inheritdoc />
    public ErrorOr<Beneficiary> FindById(int beneficiaryId)
    {
        Beneficiary? beneficiary = _databaseContext.Beneficiaries
            .AsNoTracking()
            .FirstOrDefault(beneficiary => beneficiary.Id == beneficiaryId);

        if (beneficiary is null)
        {
            return Error.NotFound(
                code: "Beneficiary.NotFound",
                description: $"Beneficiary with id '{beneficiaryId}' was not found.");
        }

        return beneficiary;
    }

    /// <inheritdoc />
    public ErrorOr<List<Beneficiary>> FindByUserId(int userId)
    {
        List<Beneficiary> beneficiaries = _databaseContext.Beneficiaries
            .AsNoTracking()
            .Where(beneficiary => beneficiary.UserId == userId)
            .OrderBy(beneficiary => beneficiary.Name)
            .ToList();

        return beneficiaries;
    }

    /// <inheritdoc />
    public ErrorOr<bool> ExistsByUserIdAndIban(int userId, string iban)
    {
        bool exists = _databaseContext.Beneficiaries
            .AsNoTracking()
            .Any(beneficiary =>
                beneficiary.UserId == userId &&
                beneficiary.Iban.ToLower() == iban.ToLower());

        return exists;
    }

    /// <inheritdoc />
    public ErrorOr<Beneficiary> Create(Beneficiary beneficiary)
    {
        try
        {
            _databaseContext.Beneficiaries.Add(beneficiary);
            _databaseContext.SaveChanges();
            return beneficiary;
        }
        catch (DbUpdateException databaseUpdateException)
        {
            return Error.Failure(
                code: "Beneficiary.CreateFailed",
                description: $"Failed to create beneficiary: {databaseUpdateException.Message}");
        }
    }

    /// <inheritdoc />
    public ErrorOr<Success> Update(Beneficiary beneficiary)
    {
        bool exists = _databaseContext.Beneficiaries
            .AsNoTracking()
            .Any(existingBeneficiary => existingBeneficiary.Id == beneficiary.Id);

        if (!exists)
        {
            return Error.NotFound(
                code: "Beneficiary.NotFound",
                description: $"Beneficiary with id '{beneficiary.Id}' was not found.");
        }

        try
        {
            _databaseContext.Beneficiaries.Update(beneficiary);
            _databaseContext.SaveChanges();
            return Result.Success;
        }
        catch (DbUpdateException databaseUpdateException)
        {
            return Error.Failure(
                code: "Beneficiary.UpdateFailed",
                description: $"Failed to update beneficiary: {databaseUpdateException.Message}");
        }
    }

    /// <inheritdoc />
    public ErrorOr<Success> Delete(int beneficiaryId)
    {
        Beneficiary? beneficiary = _databaseContext.Beneficiaries
            .FirstOrDefault(existingBeneficiary => existingBeneficiary.Id == beneficiaryId);

        if (beneficiary is null)
        {
            return Error.NotFound(
                code: "Beneficiary.NotFound",
                description: $"Beneficiary with id '{beneficiaryId}' was not found.");
        }

        try
        {
            _databaseContext.Beneficiaries.Remove(beneficiary);
            _databaseContext.SaveChanges();
            return Result.Success;
        }
        catch (DbUpdateException databaseUpdateException)
        {
            return Error.Failure(
                code: "Beneficiary.DeleteFailed",
                description: $"Failed to delete beneficiary: {databaseUpdateException.Message}");
        }
    }
}