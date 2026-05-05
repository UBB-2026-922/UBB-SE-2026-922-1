// <copyright file="SaveBillerDto.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the SaveBillerDto class.
// </summary>

namespace BankingApp.Application.DTOs.BillPayments;

/// <summary>
/// Data transfer object used for saving a biller to a user's quick list.
/// </summary>
public class SaveBillerDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the biller to save.
    /// </summary>
    public int BillerId { get; set; }

    /// <summary>
    /// Gets or sets a personal nickname for the saved biller.
    /// </summary>
    required public string Nickname { get; set; }
}
