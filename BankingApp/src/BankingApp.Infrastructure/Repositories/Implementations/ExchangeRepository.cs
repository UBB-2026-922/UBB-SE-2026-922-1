// <copyright file="ExchangeRepository.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the dummy ExchangeRepository skeleton for Team B integration.
// </summary>

using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Domain.Entities;
using ErrorOr;

namespace BankingApp.Infrastructure.Repositories.Implementations;

/// <summary>
///     Skeleton implementation of <see cref="IExchangeRepository" />.
///     All members throw <see cref="NotImplementedException" /> and serve as
///     landing zones for the Team B integration task.
/// </summary>
public class ExchangeRepository : IExchangeRepository
{
    /// <inheritdoc />
    public ErrorOr<ExchangeTransaction> GetById(int id)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<List<ExchangeTransaction>> GetByUserId(int userId)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<ExchangeTransaction> Create(ExchangeTransaction exchange)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<ExchangeTransaction> UpdateStatus(int exchangeId, Domain.Enums.ExchangeTransactionStatus status)
    {
        throw new NotImplementedException();
    }
}
