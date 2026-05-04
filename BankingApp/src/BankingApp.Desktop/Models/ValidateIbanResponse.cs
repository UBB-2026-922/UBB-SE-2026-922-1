// <copyright file="ValidateIbanResponse.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the ValidateIbanResponse class.
// </summary>

namespace BankingApp.Desktop.Models;

/// <summary>
///     Response from the IBAN validation endpoint.
/// </summary>
public class ValidateIbanResponse
{
    /// <summary>
    ///     Gets or sets a value indicating whether the IBAN is structurally valid.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool IsValid { get; set; }

    /// <summary>
    ///     Gets or sets the bank name inferred from the IBAN country code.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string BankName { get; set; } = string.Empty;
}
