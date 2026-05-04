// <copyright file="BillerDataAccess.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the BillerDataAccess class.
// </summary>

using BankingApp.Domain.Entities;
using BankingApp.Domain.Errors;
using BankingApp.Infrastructure.DataAccess.Interfaces;
using ErrorOr;

namespace BankingApp.Infrastructure.DataAccess.Implementations;

/// <summary>
///     Provides EF Core data access for billers.
/// </summary>
public class BillerDataAccess : IBillerDataAccess
{
    private readonly AppDatabaseContext _databaseContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BillerDataAccess" /> class.
    /// </summary>
    /// <param name="databaseContext">The database context.</param>
    public BillerDataAccess(AppDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    /// <inheritdoc />
    public ErrorOr<List<Biller>> GetAll(bool activeOnly)
    {
        IQueryable<Biller> query = _databaseContext.Billers;
        if (activeOnly)
        {
            query = query.Where(b => b.IsActive);
        }

        return query.OrderBy(b => b.Name).ToList();
    }

    /// <inheritdoc />
    public ErrorOr<List<Biller>> Search(string searchTerm, string? category, bool activeOnly)
    {
        IQueryable<Biller> query = _databaseContext.Billers;
        if (activeOnly)
        {
            query = query.Where(b => b.IsActive);
        }

        query = query.Where(b => b.Name.Contains(searchTerm));
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(b => b.Category == category);
        }

        return query.OrderBy(b => b.Name).ToList();
    }

    /// <inheritdoc />
    public ErrorOr<Biller> FindById(int id)
    {
        Biller? biller = _databaseContext.Billers.FirstOrDefault(b => b.Id == id);
        if (biller is null)
        {
            return BillerErrors.BillerNotFound;
        }

        return biller;
    }
}
