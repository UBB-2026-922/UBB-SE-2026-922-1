// <copyright file="RegistrationContext.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the RegistrationContext class.
// </summary>

namespace BankingApp.Desktop.Utilities;

/// <inheritdoc />
public class RegistrationContext : IRegistrationContext
{
    /// <inheritdoc />
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public bool JustRegistered { get; set; }
}