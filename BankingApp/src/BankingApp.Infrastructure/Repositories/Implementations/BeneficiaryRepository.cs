// <copyright file="BeneficiaryRepository.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the dummy BeneficiaryRepository skeleton for Team B integration.
// </summary>

using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Domain.Entities;
using ErrorOr;

namespace BankingApp.Infrastructure.Repositories.Implementations;

/// <summary>
///     Skeleton implementation of <see cref="IBeneficiaryRepository" />.
///     All members throw <see cref="NotImplementedException" /> and serve as
///     landing zones for the Team B integration task.
/// </summary>
public class BeneficiaryRepository : IBeneficiaryRepository
{
    /// <inheritdoc />
    public ErrorOr<Beneficiary> GetById(int id)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<List<Beneficiary>> GetByUserId(int userId)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<Beneficiary> Create(Beneficiary beneficiary)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<Beneficiary> Update(Beneficiary beneficiary)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public ErrorOr<Success> Delete(int id)
    {
        throw new NotImplementedException();
    }
}
