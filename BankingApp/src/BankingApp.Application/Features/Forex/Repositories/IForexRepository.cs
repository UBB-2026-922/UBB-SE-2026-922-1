namespace BankingApp.Application.Features.Forex.Repositories;

using Domain.Aggregates.ForexAggregate;
using Domain.Entities;
using Domain.Enums;
using ErrorOr;

/// <summary>
///     Defines persistence operations for the <see cref="ForexTransaction" /> entity.
/// </summary>
public interface IForexRepository
{
    /// <summary>Retrieves an exchange transaction by its unique identifier.</summary>
    /// <param name="id">The exchange transaction identifier.</param>
    /// <returns>The matching <see cref="ForexTransaction" />, or an error when not found.</returns>
    public ErrorOr<ForexTransaction> GetById(int id);

    /// <summary>Retrieves all exchange transactions belonging to the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of exchange transactions, or an error.</returns>
    public ErrorOr<List<ForexTransaction>> GetByUserId(int userId);

    /// <summary>Persists a new exchange transaction record.</summary>
    /// <param name="forex">The exchange transaction to create.</param>
    /// <returns>The created record with its assigned identifier, or an error.</returns>
    public ErrorOr<ForexTransaction> Create(ForexTransaction forex);

    /// <summary>Updates the processing status of an existing exchange transaction.</summary>
    /// <param name="exchangeId">The identifier of the exchange transaction to update.</param>
    /// <param name="status">The new status value.</param>
    /// <returns>The updated record, or an error.</returns>
    public ErrorOr<ForexTransaction> UpdateStatus(int exchangeId, ExchangeTransactionStatus status);
}
