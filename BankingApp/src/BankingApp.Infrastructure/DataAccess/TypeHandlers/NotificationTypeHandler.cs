// <copyright file="NotificationTypeHandler.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the NotificationTypeHandler class.
// </summary>

using System.Data;
using BankingApp.Domain.Enums;
using BankingApp.Domain.Extensions;
using Dapper;

namespace BankingApp.Infrastructure.DataAccess.TypeHandlers;

/// <summary>
///     Dapper type handler for <see cref="NotificationType" />.
///     The database stores display names (e.g. "Outbound Transfer") rather than
///     enum member names, so the generic <see cref="EnumTypeHandler{T}" /> cannot be used.
/// </summary>
public class NotificationTypeHandler : SqlMapper.TypeHandler<NotificationType>
{
    /// <inheritdoc />
    /// <param name="value">The value value.</param>
    /// <returns>The result of the operation.</returns>
    public override NotificationType Parse(object value)
    {
        return NotificationTypeExtensions.FromString((string)value);
    }

    /// <inheritdoc />
    /// <param name="parameter">The parameter value.</param>
    /// <param name="value">The value value.</param>
    public override void SetValue(IDbDataParameter parameter, NotificationType value)
    {
        parameter.Value = value.ToDisplayName();
    }
}