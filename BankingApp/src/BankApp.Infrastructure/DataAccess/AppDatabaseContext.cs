// <copyright file="AppDatabaseContext.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the AppDatabaseContext class.
// </summary>

using System.Data;
using ErrorOr;
using Microsoft.Data.SqlClient;

namespace BankApp.Infrastructure.DataAccess;

/// <summary>
///     Provides a concrete implementation of <see cref="IDatabaseContext" /> using SQL Server via
///     <see cref="SqlConnection" />.
/// </summary>
public class AppDatabaseContext : IDatabaseContext
{
    private readonly string _connectionString;
    private SqlConnection? _connection;
    private SqlTransaction? _currentTransaction;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AppDatabaseContext" /> class.
    /// </summary>
    /// <param name="connectionString">The SQL Server _connection string.</param>
    /// <returns>The result of the operation.</returns>
    public AppDatabaseContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public ErrorOr<T> Query<T>(Func<SqlConnection, T> operation)
    {
        try
        {
            T result = operation(GetConnection());
            if (result is null)
            {
                return Error.NotFound(description: "No record found.");
            }

            return result;
        }
        catch (Exception exception)
        {
            return Error.Failure(description: exception.Message);
        }
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    public ErrorOr<SqlTransaction> BeginTransaction()
    {
        SqlConnection activeConnection = GetConnection();
        try
        {
            _currentTransaction = activeConnection.BeginTransaction();
        }
        catch (Exception exception) when (exception is SqlException or InvalidOperationException)
        {
            return Error.Failure(description: $"Failed to begin transaction: {exception.Message}");
        }

        return _currentTransaction;
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> CommitTransaction()
    {
        if (_currentTransaction is null)
        {
            return Error.Conflict(description: "No active transaction to commit.");
        }

        _currentTransaction.Commit();
        _currentTransaction = null;
        return Result.Success;
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> RollbackTransaction()
    {
        if (_currentTransaction is null)
        {
            return Error.Conflict(description: "No active transaction to rollback.");
        }

        _currentTransaction.Rollback();
        _currentTransaction = null;
        return Result.Success;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _currentTransaction?.Dispose();
        if (_connection is null)
        {
            return;
        }

        if (_connection.State != ConnectionState.Closed)
        {
            _connection.Close();
        }

        _connection.Dispose();
        _connection = null;
    }

    private SqlConnection GetConnection()
    {
        if (_connection is not null && _connection.State is not ConnectionState.Closed)
        {
            return _connection;
        }

        _connection = new SqlConnection(_connectionString);
        _connection.Open();
        return _connection;
    }
}