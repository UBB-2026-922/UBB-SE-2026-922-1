// <copyright file="IRecurringPaymentService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the IRecurringPaymentService interface.
// </summary>

using BankingApp.Application.DTOs.RecurringPayments;
using ErrorOr;

namespace BankingApp.Application.Services.RecurringPayments;

/// <summary>
///     Defines business operations for managing recurring payment schedules.
/// </summary>
public interface IRecurringPaymentService
{
    /// <summary>
    ///     Creates a new recurring payment schedule for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the authenticated user creating the schedule.</param>
    /// <param name="request">The creation request containing schedule details.</param>
    /// <returns>
    ///     A <see cref="RecurringPaymentResponse" /> for the newly created schedule,
    ///     or <see cref="ErrorType.Validation" /> if the request contains invalid data,
    ///     or <see cref="ErrorType.Failure" /> if the persistence operation failed.
    /// </returns>
    ErrorOr<RecurringPaymentResponse> Create(int userId, CreateRecurringPaymentRequest request);

    /// <summary>
    ///     Returns all recurring payment schedules owned by the specified user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>
    ///     A list of <see cref="RecurringPaymentResponse" /> DTOs,
    ///     or <see cref="ErrorType.Failure" /> if the query failed.
    /// </returns>
    ErrorOr<List<RecurringPaymentResponse>> GetByUser(int userId);

    /// <summary>
    ///     Pauses an active recurring payment schedule.
    /// </summary>
    /// <param name="userId">The identifier of the requesting user, used to verify ownership.</param>
    /// <param name="id">The recurring payment identifier to pause.</param>
    /// <returns>
    ///     Success, or <see cref="Error.NotFound" /> if the schedule does not exist,
    ///     or <see cref="ErrorType.Forbidden" /> if the user does not own the schedule.
    /// </returns>
    ErrorOr<Success> Pause(int userId, int id);

    /// <summary>
    ///     Resumes a paused recurring payment schedule.
    /// </summary>
    /// <param name="userId">The identifier of the requesting user, used to verify ownership.</param>
    /// <param name="id">The recurring payment identifier to resume.</param>
    /// <returns>
    ///     Success, or <see cref="Error.NotFound" /> if the schedule does not exist,
    ///     or <see cref="ErrorType.Forbidden" /> if the user does not own the schedule.
    /// </returns>
    ErrorOr<Success> Resume(int userId, int id);

    /// <summary>
    ///     Permanently cancels a recurring payment schedule.
    /// </summary>
    /// <param name="userId">The identifier of the requesting user, used to verify ownership.</param>
    /// <param name="id">The recurring payment identifier to cancel.</param>
    /// <returns>
    ///     Success, or <see cref="Error.NotFound" /> if the schedule does not exist,
    ///     or <see cref="ErrorType.Forbidden" /> if the user does not own the schedule.
    /// </returns>
    ErrorOr<Success> Cancel(int userId, int id);
}
