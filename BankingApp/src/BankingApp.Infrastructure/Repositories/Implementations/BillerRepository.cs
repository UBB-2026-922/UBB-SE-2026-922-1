// <copyright file="BillerRepository.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the BillerRepository class.
// </summary>

using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.DataAccess.Interfaces;
using ErrorOr;

namespace BankingApp.Infrastructure.Repositories.Implementations;

/// <summary>
///     Provides repository operations for billers and saved billers.
/// </summary>
public class BillerRepository : IBillerRepository
{
    private readonly IBillerDataAccess _billerDataAccess;
    private readonly ISavedBillerDataAccess _savedBillerDataAccess;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BillerRepository" /> class.
    /// </summary>
    /// <param name="billerDataAccess">The biller data access component.</param>
    /// <param name="savedBillerDataAccess">The saved biller data access component.</param>
    public BillerRepository(IBillerDataAccess billerDataAccess, ISavedBillerDataAccess savedBillerDataAccess)
    {
        _billerDataAccess = billerDataAccess;
        _savedBillerDataAccess = savedBillerDataAccess;
    }

    /// <inheritdoc />
    public ErrorOr<List<Biller>> GetAllBillers(bool activeOnly = true) =>
        _billerDataAccess.GetAll(activeOnly);

    /// <inheritdoc />
    public ErrorOr<List<Biller>> SearchBillers(string searchTerm, string? category = null, bool activeOnly = true) =>
        _billerDataAccess.Search(searchTerm, category, activeOnly);

    /// <inheritdoc />
    public ErrorOr<Biller> GetBillerById(int billerId) =>
        _billerDataAccess.FindById(billerId);

    /// <inheritdoc />
    public ErrorOr<List<SavedBiller>> GetSavedBillers(int userId) =>
        _savedBillerDataAccess.FindByUserId(userId);

    /// <inheritdoc />
    public ErrorOr<SavedBiller> SaveBiller(SavedBiller savedBiller) =>
        _savedBillerDataAccess.Add(savedBiller);

    /// <inheritdoc />
    public ErrorOr<Success> DeleteSavedBiller(int savedBillerId) =>
        _savedBillerDataAccess.Delete(savedBillerId);
}
