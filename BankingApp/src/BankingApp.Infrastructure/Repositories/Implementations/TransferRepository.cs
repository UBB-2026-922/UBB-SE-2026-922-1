// <copyright file="TransferRepository.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the dummy TransferRepository skeleton for Team B integration.
// </summary>

using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Domain.Entities;
using ErrorOr;

namespace BankingApp.Infrastructure.Repositories.Implementations;

/// <summary>
///     Skeleton implementation of <see cref="ITransferRepository" />.
///     All members throw <see cref="NotImplementedException" /> and serve as
///     landing zones for the Team B integration task.
/// </summary>
public class TransferRepository : ITransferRepository
{
    /// <inheritdoc />
    public ErrorOr<Transfer> GetById(int id)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<List<Transfer>> GetByUserId(int userId)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<Transfer> Create(Transfer transfer)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<Transfer> UpdateStatus(int transferId, Domain.Enums.TransferStatus status)
    {
        throw new NotImplementedException();
    }
}
