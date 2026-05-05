// <copyright file="SavedBillerDataAccess.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the SavedBillerDataAccess class.
// </summary>

using BankingApp.Domain.Entities;
using BankingApp.Domain.Errors;
using BankingApp.Infrastructure.DataAccess.Interfaces;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Infrastructure.DataAccess.Implementations;

/// <summary>
///     Provides EF Core data access for saved billers.
/// </summary>
public class SavedBillerDataAccess : ISavedBillerDataAccess
{
    private readonly AppDatabaseContext _databaseContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SavedBillerDataAccess" /> class.
    /// </summary>
    /// <param name="databaseContext">The database context.</param>
    public SavedBillerDataAccess(AppDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    /// <inheritdoc />
    public ErrorOr<List<SavedBiller>> FindByUserId(int userId)
    {
        return _databaseContext.SavedBillers
            .Include(savedBiller => savedBiller.Biller)
            .Where(savedBiller => savedBiller.UserId == userId)
            .OrderByDescending(savedBiller => savedBiller.CreatedAt)
            .ToList();
    }

    /// <inheritdoc />
    public ErrorOr<SavedBiller> Add(SavedBiller savedBiller)
    {
        _databaseContext.SavedBillers.Add(savedBiller);
        _databaseContext.SaveChanges();
        return savedBiller;
    }

    /// <inheritdoc />
    public ErrorOr<Success> Delete(int id)
    {
        SavedBiller? entry = _databaseContext.SavedBillers.FirstOrDefault(savedBiller => savedBiller.Id == id);
        if (entry is null)
        {
            return BillerErrors.SavedBillerNotFound;
        }

        _databaseContext.SavedBillers.Remove(entry);
        _databaseContext.SaveChanges();
        return Result.Success;
    }
}
