// <copyright file="DatabaseFixture.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankingApp.Infrastructure.DataAccess;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Respawn;
using Testcontainers.MsSql;

namespace BankingApp.Infrastructure.Tests.Integration.Infrastructure;

/// <summary>
///     Provides a repeatable SQL Server database via Testcontainers for integration tests.
///     Each test class that implements <see cref="IClassFixture{DatabaseFixture}" />
///     shares one MsSqlContainer for the lifetime of that class.
///     Call <see cref="ResetAsync" /> before each test to wipe all data cleanly.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global — xUnit instantiates fixtures via reflection.
public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _databaseContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest")
        .Build();

    private string _connectionString = string.Empty;
    private SqlConnection? _connection;
    private Respawner? _respawner;

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await _databaseContainer.StartAsync();

        _connectionString = _databaseContainer.GetConnectionString();
        await using AppDatabaseContext databaseContext = CreateDatabaseContext();
        await databaseContext.Database.MigrateAsync();

        _connection = new SqlConnection(_connectionString);
        await _connection.OpenAsync();

        const string schemaName = "dbo";

        _respawner = await Respawner.CreateAsync(
            _connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                SchemasToInclude = [schemaName],
            });
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }

        await _databaseContainer.DisposeAsync();
    }

    /// <summary>
    ///     Creates a fresh <see cref="AppDatabaseContext" /> connected to the Testcontainers SQL Server instance.
    /// </summary>
    /// <returns>A new <see cref="AppDatabaseContext" />.</returns>
    public AppDatabaseContext CreateDatabaseContext()
    {
        DbContextOptions<AppDatabaseContext> options = new DbContextOptionsBuilder<AppDatabaseContext>()
            .UseSqlServer(_connectionString)
            .Options;
        return new AppDatabaseContext(options);
    }

    /// <summary>
    ///     Wipes all data from the database using Respawn.
    ///     Should be called before each test run to ensure a clean state.
    /// </summary>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async Task ResetAsync()
    {
        if (_respawner != null && _connection != null)
        {
            await _respawner.ResetAsync(_connection);
        }
    }
}
