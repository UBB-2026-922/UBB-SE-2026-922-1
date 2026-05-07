// <copyright file="IBillPaymentService.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the IBillPaymentService interface.
// </summary>

using BankingApp.Application.DTOs.TeamB;
using ErrorOr;

namespace BankingApp.Application.Services.TeamB;

/// <summary>
///     Defines application-level operations for the bill payment feature.
/// </summary>
public interface IBillPaymentService
{
    /// <summary>Retrieves the list of all registered billers.</summary>
    /// <returns>A list of biller DTOs, or an error.</returns>
    ErrorOr<List<BillerDto>> GetBillers();

    /// <summary>Retrieves all bill payments made by the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>A list of bill payment response DTOs, or an error.</returns>
    ErrorOr<List<BillPaymentResponseDto>> GetPaymentHistory(int userId);

    /// <summary>Processes a one-off bill payment.</summary>
    /// <param name="request">The payment details.</param>
    /// <returns>The created payment response DTO, or an error.</returns>
    ErrorOr<BillPaymentResponseDto> PayBill(BillPaymentRequestDto request);
}