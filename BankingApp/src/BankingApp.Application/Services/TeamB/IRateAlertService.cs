// <copyright file="IRateAlertService.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the IRateAlertService interface.
// </summary>

using BankingApp.Application.DTOs.TeamB;
using ErrorOr;

namespace BankingApp.Application.Services.TeamB;

/// <summary>
///     Defines application-level operations for managing FX rate alerts.
/// </summary>
public interface IRateAlertService
{
    /// <summary>Retrieves all rate alerts for the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of rate alert DTOs, or an error.</returns>
    ErrorOr<List<RateAlertDto>> GetAlerts(int userId);

    /// <summary>Creates a new rate alert for a currency pair.</summary>
    /// <param name="dto">The alert configuration.</param>
    /// <returns>The created alert DTO, or an error.</returns>
    ErrorOr<RateAlertDto> CreateAlert(RateAlertDto dto);

    /// <summary>Removes a rate alert.</summary>
    /// <param name="id">The alert identifier.</param>
    /// <returns>Success, or an error when not found.</returns>
    ErrorOr<Success> DeleteAlert(int id);

    /// <summary>Checks all untriggered alerts against current rates and triggers those that match.</summary>
    /// <returns>The number of alerts triggered, or an error.</returns>
    ErrorOr<int> ProcessAlerts();
}