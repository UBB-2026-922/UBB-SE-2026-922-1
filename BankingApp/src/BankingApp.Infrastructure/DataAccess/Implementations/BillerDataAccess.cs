namespace BankingApp.Infrastructure.DataAccess.Implementations;

using Domain.Entities;
using Domain.Errors;
using Interfaces;
using ErrorOr;

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
            query = query.Where(biller => biller.IsActive);
        }

        return query.OrderBy(biller => biller.Name).ToList();
    }

    /// <inheritdoc />
    public ErrorOr<List<Biller>> Search(string searchTerm, string? category, bool activeOnly)
    {
        IQueryable<Biller> query = _databaseContext.Billers;
        if (activeOnly)
        {
            query = query.Where(biller => biller.IsActive);
        }

        query = query.Where(biller => biller.Name.Contains(searchTerm));
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(biller => biller.Category == category);
        }

        return query.OrderBy(biller => biller.Name).ToList();
    }

    /// <inheritdoc />
    public ErrorOr<Biller> FindById(int id)
    {
        Biller? biller = _databaseContext.Billers.FirstOrDefault(candidateBiller => candidateBiller.Id == id);
        if (biller is null)
        {
            return BillerErrors.BillerNotFound;
        }

        return biller;
    }
}
