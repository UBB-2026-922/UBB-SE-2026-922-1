// <copyright file="IRateAlertRepository.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the IRateAlertRepository interface.
// </summary>

using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.TeamB;
using ErrorOr;

namespace BankingApp.Desktop.Repositories;

/// <summary>
///     Defines the data-access contract for rate alert operations
///     on the client side. Decouples ViewModels from the HTTP transport layer.
/// </summary>
public interface IRateAlertRepository
{
    /// <summary>
    ///     Retrieves all rate alerts for the specified user.
    /// </summary>
    /// <param name="userId">The owning user's identifier.</param>
    /// <returns>The list of alerts, or an error if the request fails.</returns>
    Task<ErrorOr<List<RateAlertDto>>> GetAlertsAsync(int userId);

    /// <summary>
    ///     Creates a new rate alert.
    /// </summary>
    /// <param name="alert">The alert configuration to persist.</param>
    /// <returns>The created alert with its assigned identifier, or an error.</returns>
    Task<ErrorOr<RateAlertDto>> CreateAlertAsync(RateAlertDto alert);

    /// <summary>
    ///     Deletes an existing rate alert.
    /// </summary>
    /// <param name="alertId">The identifier of the alert to remove.</param>
    /// <returns>Success, or an error if the request fails.</returns>
    Task<ErrorOr<Success>> DeleteAlertAsync(int alertId);
}
