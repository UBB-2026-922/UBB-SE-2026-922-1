using BankingApp.Domain.Entities;
using ErrorOr;

namespace BankingApp.Infrastructure.DataAccess.Interfaces;

/// <summary>
///     Defines low-level data access operations for billers.
/// </summary>
public interface IBillerDataAccess
{
    /// <summary>Returns all billers, optionally filtered to active-only.</summary>
    ErrorOr<List<Biller>> GetAll(bool activeOnly);

    /// <summary>Searches billers by name fragment and optional category.</summary>
    ErrorOr<List<Biller>> Search(string searchTerm, string? category, bool activeOnly);

    /// <summary>Finds a biller by its unique identifier.</summary>
    ErrorOr<Biller> FindById(int id);
}
