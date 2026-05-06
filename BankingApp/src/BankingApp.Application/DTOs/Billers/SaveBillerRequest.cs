// <copyright file="SaveBillerRequest.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the SaveBillerRequest class.
// </summary>

namespace BankingApp.Application.DTOs.Billers;

/// <summary>
///     Request body for saving a biller.
/// </summary>
public class SaveBillerRequest
{
    /// <summary>Gets or sets the identifier of the biller to save.</summary>
    /// <value>Gets or sets the current value.</value>
    public int BillerId { get; set; }

    /// <summary>Gets or sets the optional user-assigned nickname.</summary>
    /// <value>Gets or sets the current value.</value>
    public string? Nickname { get; set; }

    /// <summary>Gets or sets the optional default payment reference.</summary>
    /// <value>Gets or sets the current value.</value>
    public string? DefaultReference { get; set; }
}