// <copyright file="CreateTransferRequest.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the TCreateTransferRequest class.
// </summary>

namespace BankingApp.Application.DataTransferObjects.Dashboard;

/// <summary>
/// Represents a request to initiate a new fund transfer from the dashboard.
/// </summary>
public class CreateTransferRequest
{
    /// <summary>
    /// Gets or sets the ID of the source account owned by the user.
    /// </summary>
    public int SourceAccountId { get; set; }

    /// <summary>
    /// Gets or sets the recipient's IBAN (can be internal or external).
    /// </summary>
    public string DestinationIban { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the amount to transfer.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency (e.g., "RON").
    /// </summary>
    public string Currency { get; set; } = "RON";

    /// <summary>
    /// Gets or sets the transfer remarks or reference.
    /// </summary>
    public string? Description { get; set; }
}