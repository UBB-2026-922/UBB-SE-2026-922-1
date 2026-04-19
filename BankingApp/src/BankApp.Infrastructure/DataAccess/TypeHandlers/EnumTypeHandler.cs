// <copyright file="EnumTypeHandler.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the EnumTypeHandler class.
// </summary>

using System.Data;
using Dapper;

namespace BankApp.Infrastructure.DataAccess.TypeHandlers;

/// <summary>
///     Convert SQL types to corresponding Enum.
/// </summary>
/// <typeparam name="T">The enum to handle.</typeparam>
public class EnumTypeHandler<T> : SqlMapper.TypeHandler<T>
    where T : struct, Enum
{
    /// <inheritdoc />
    /// <param name="parameter">The parameter value.</param>
    /// <param name="value">The value value.</param>
    public override void SetValue(IDbDataParameter parameter, T value)
    {
        parameter.Value = value.ToString();
    }

    /// <inheritdoc />
    /// <param name="value">The value value.</param>
    /// <returns>The parsed enum value.</returns>
    public override T Parse(object value)
    {
        return Enum.Parse<T>((string)value, true);
    }
}