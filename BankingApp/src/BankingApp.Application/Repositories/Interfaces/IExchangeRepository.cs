// <copyright file="IExchangeRepository.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the IExchangeRepository interface.
// </summary>

using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using ErrorOr;

namespace BankingApp.Application.Repositories.Interfaces;

/// <summary>
///     Defines persistence operations for the <see cref="ExchangeTransaction" /> entity.
/// </summary>
public interface IExchangeRepository
{
    /// <summary>Retrieves an exchange transaction by its unique identifier.</summary>
    /// <param name="id">The exchange transaction identifier.</param>
    /// <returns>The matching <see cref="ExchangeTransaction" />, or an error when not found.</returns>
    ErrorOr<ExchangeTransaction> GetById(int id);

    /// <summary>Retrieves all exchange transactions belonging to the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of exchange transactions, or an error.</returns>
    ErrorOr<List<ExchangeTransaction>> GetByUserId(int userId);

    /// <summary>Persists a new exchange transaction record.</summary>
    /// <param name="exchange">The exchange transaction to create.</param>
    /// <returns>The created record with its assigned identifier, or an error.</returns>
    ErrorOr<ExchangeTransaction> Create(ExchangeTransaction exchange);

    /// <summary>Updates the processing status of an existing exchange transaction.</summary>
    /// <param name="exchangeId">The identifier of the exchange transaction to update.</param>
    /// <param name="status">The new status value.</param>
    /// <returns>The updated record, or an error.</returns>
    ErrorOr<ExchangeTransaction> UpdateStatus(int exchangeId, ExchangeTransactionStatus status);
}
