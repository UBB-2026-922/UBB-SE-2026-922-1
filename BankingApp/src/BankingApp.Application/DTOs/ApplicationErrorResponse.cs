// <copyright file="ApplicationErrorResponse.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains code for ApplicationErrorResponse.
// </summary>

namespace BankingApp.Application.DTOs;

/// <summary>
///     Represents the JSON error body returned by all API endpoints on failure.
///     The HTTP status code signals the error category; this body carries the
///     human-readable message and a machine-readable code for client branching logic.
/// </summary>
public sealed class ApplicationErrorResponse
{
    /// <summary>
    ///     Gets or sets the human-readable error description.
    /// </summary>
    public string Error { get; init; } = string.Empty;

    /// <summary>
    ///     Gets or sets a machine-readable error code for client branching
    ///     (e.g. <c>"token_expired"</c>, <c>"account_locked"</c>).
    ///     Empty string when no specific code applies.
    /// </summary>
    public string ErrorCode { get; init; } = string.Empty;
}