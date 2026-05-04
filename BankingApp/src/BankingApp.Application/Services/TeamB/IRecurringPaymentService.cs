// <copyright file="IRecurringPaymentService.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the IRecurringPaymentService interface.
// </summary>

using BankingApp.Application.DTOs.TeamB;
using ErrorOr;

namespace BankingApp.Application.Services.TeamB;

/// <summary>
///     Defines application-level operations for managing recurring payment schedules.
/// </summary>
public interface IRecurringPaymentService
{
    /// <summary>Retrieves all recurring payment schedules for the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of recurring payment DTOs, or an error.</returns>
    ErrorOr<List<RecurringPaymentDto>> GetSchedules(int userId);

    /// <summary>Creates a new recurring payment schedule.</summary>
    /// <param name="dto">The schedule details.</param>
    /// <returns>The created schedule DTO, or an error.</returns>
    ErrorOr<RecurringPaymentDto> CreateSchedule(RecurringPaymentDto dto);

    /// <summary>Cancels an active recurring payment schedule.</summary>
    /// <param name="id">The schedule identifier.</param>
    /// <returns>Success, or an error when not found.</returns>
    ErrorOr<Success> CancelSchedule(int id);

    /// <summary>Executes all due recurring payments as of the current UTC time.</summary>
    /// <returns>The number of payments executed, or an error.</returns>
    ErrorOr<int> ProcessDuePayments();
}
