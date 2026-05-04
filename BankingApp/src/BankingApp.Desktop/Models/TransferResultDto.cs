// <copyright file="TransferResultDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the TransferResultDto class.
// </summary>

namespace BankingApp.Desktop.Models;

/// <summary>
///     Response body returned by the transfer execution endpoint on success.
/// </summary>
public class TransferResultDto
{
    /// <summary>
    ///     Gets or sets the human-readable transaction reference for the completed transfer.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string TransactionRef { get; set; } = string.Empty;
}
