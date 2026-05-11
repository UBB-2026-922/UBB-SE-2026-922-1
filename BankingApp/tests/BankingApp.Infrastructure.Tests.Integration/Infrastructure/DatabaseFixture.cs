namespace BankingApp.Infrastructure.Tests.Integration.Infrastructure;

using BankingApp.Infrastructure.DataAccess;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Persistence;
using Respawn;
using Testcontainers.MsSql;

/// <summary>
///     Provides a repeatable SQL Server database via Testcontainers for integration tests.
///     Each test class that implements <see cref="IClassFixture{DatabaseFixture}" />
///     shares one MsSqlContainer for the lifetime of that class.
///     Call <see cref="ResetAsync" /> before each test to wipe all data cleanly.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global - xUnit instantiates fixtures via reflection.
public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer? _databaseContainer;
    private readonly string? _externalConnectionString =
        Environment.GetEnvironmentVariable("MSSQL_CONNECTION_STRING");

    private string _connectionString = string.Empty;
    private SqlConnection? _connection;
    private Respawner? _respawner;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DatabaseFixture" /> class.
    /// </summary>
    public DatabaseFixture()
    {
        if (_externalConnectionString is null)
        {
            _databaseContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
                .Build();
        }
    }

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        if (_databaseContainer is not null)
        {
            await _databaseContainer.StartAsync();
            _connectionString = _databaseContainer.GetConnectionString();
        }
        else
        {
            _connectionString = _externalConnectionString!;
        }

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
                SchemasToInclude = [schemaName]
            });
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }

        if (_databaseContainer is not null)
        {
            await _databaseContainer.DisposeAsync();
        }
    }

    /// <summary>
    ///     Creates a fresh <see cref="AppDatabaseContext" /> connected to the Testcontainers SQL Server instance.
    /// </summary>
    /// <returns>A new <see cref="AppDatabaseContext" />.</returns>
    public AppDatabaseContext CreateDatabaseContext()
    {
        DbContextOptions<AppDatabaseContext> options = new DbContextOptionsBuilder<AppDatabaseContext>()
            .UseSqlServer(_connectionString)
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
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
