// <copyright file="RegistrationContext.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the RegistrationContext class.
// </summary>

namespace BankApp.Desktop.Utilities;

/// <inheritdoc />
public class RegistrationContext : IRegistrationContext
{
    /// <inheritdoc />
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool JustRegistered { get; set; }
}