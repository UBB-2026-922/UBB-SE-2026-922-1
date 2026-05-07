namespace BankingApp.Infrastructure.Repositories.Implementations;

using BankingApp.Application.Repositories.Interfaces;
using Domain.Entities;
using DataAccess.Interfaces;
using ErrorOr;

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
    public ErrorOr<List<Biller>> GetAllBillers(bool activeOnly = true)
    {
        return _billerDataAccess.GetAll(activeOnly);
    }

    /// <inheritdoc />
    public ErrorOr<List<Biller>> SearchBillers(string searchTerm, string? category = null, bool activeOnly = true)
    {
        return _billerDataAccess.Search(searchTerm, category, activeOnly);
    }

    /// <inheritdoc />
    public ErrorOr<Biller> GetBillerById(int billerId)
    {
        return _billerDataAccess.FindById(billerId);
    }

    /// <inheritdoc />
    public ErrorOr<List<SavedBiller>> GetSavedBillers(int userId)
    {
        return _savedBillerDataAccess.FindByUserId(userId);
    }

    /// <inheritdoc />
    public ErrorOr<SavedBiller> SaveBiller(SavedBiller savedBiller)
    {
        return _savedBillerDataAccess.Add(savedBiller);
    }

    /// <inheritdoc />
    public ErrorOr<Success> DeleteSavedBiller(int savedBillerId)
    {
        return _savedBillerDataAccess.Delete(savedBillerId);
    }
}
