// <copyright file="TransferAccountDto.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the TransferAccountDto class.
// </summary>

namespace BankingApp.Application.DTOs.TeamB;

/// <summary>
///     Data transfer object representing a bank account available for transfers.
/// </summary>
public class TransferAccountDto
{
    /// <summary>
    ///     Gets or sets the account identifier.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the account name.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the account balance.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public decimal Balance { get; set; }
}
