// <copyright file="BankingAppWebFactory.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.Dashboard;
using BankingApp.Application.Services.Login;
using BankingApp.Application.Services.PasswordRecovery;
using BankingApp.Application.Services.Profile;
using BankingApp.Application.Services.Registration;
using BankingApp.Application.Services.Security;
using BankingApp.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace BankingApp.Api.Tests.Integration.Infrastructure;

/// <summary>
///     A custom <see cref="WebApplicationFactory{TEntryPoint}" /> that:
///     <list type="bullet">
///         <item>Replaces all infrastructure services with Moq stubs.</item>
///         <item>Swaps the SQL Server <see cref="AppDatabaseContext" /> for an
///             in-memory SQLite instance so that <c>Program.cs</c> can call
///             <c>Database.Migrate()</c> without a real SQL Server connection.</item>
///     </list>
/// </summary>
public class BankingAppWebFactory : WebApplicationFactory<Program>, IDisposable
{
    // A named in-memory SQLite database that persists as long as at least one
    // connection to it is open. We keep _keepAliveConnection open for the full
    // lifetime of the factory so the schema is not discarded between requests.
    private const string SqliteConnectionString =
        "Data Source=bankingapp_api_test;Mode=Memory;Cache=Shared";

    private readonly SqliteConnection _keepAliveConnection;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BankingAppWebFactory" /> class.
    ///     Opens the keep-alive SQLite connection, creates the schema via
    ///     <c>EnsureCreated()</c>, and pre-populates <c>__EFMigrationsHistory</c>
    ///     so that <c>Program.cs</c>'s <c>Database.Migrate()</c> call is a no-op.
    /// </summary>
    public BankingAppWebFactory()
    {
        // These environment variables must be set before the host is built so that
        // AddInfrastructure does not throw during startup configuration.
        Environment.SetEnvironmentVariable("ConnectionStrings__BankingAppDb", SqliteConnectionString);
        Environment.SetEnvironmentVariable("Jwt__Secret", "integration-test-secret-that-is-long-enough-for-hmac");
        Environment.SetEnvironmentVariable("Otp__Secret", "integration-test-otp-secret-placeholder");

        // Open the keep-alive connection so the named in-memory database persists.
        _keepAliveConnection = new SqliteConnection(SqliteConnectionString);
        _keepAliveConnection.Open();

        // Build the schema and migration history before the test server starts.
        InitializeDatabase();
    }

    /// <summary>
    ///     Gets the mock JWT service that controls token validation behavior.
    /// </summary>
    public Mock<IJsonWebTokenService> JwtServiceMock { get; } = MockFactory.CreateJwtService();

    /// <summary>
    ///     Gets the mock auth repository that controls session lookup behavior.
    /// </summary>
    public Mock<IAuthRepository> AuthRepositoryMock { get; } = MockFactory.CreateAuthRepository();

    /// <summary>
    ///     Gets the mock login service.
    /// </summary>
    public Mock<ILoginService> LoginServiceMock { get; } = MockFactory.CreateLoginService();

    /// <summary>
    ///     Gets the mock registration service.
    /// </summary>
    public Mock<IRegistrationService> RegistrationServiceMock { get; } = MockFactory.CreateRegistrationService();

    /// <summary>
    ///     Gets the mock password recovery service.
    /// </summary>
    public Mock<IPasswordRecoveryService> PasswordRecoveryServiceMock { get; } =
        MockFactory.CreatePasswordRecoveryService();

    /// <summary>
    ///     Gets the mock dashboard service.
    /// </summary>
    public Mock<IDashboardService> DashboardServiceMock { get; } = MockFactory.CreateDashboardService();

    /// <summary>
    ///     Gets the mock profile service.
    /// </summary>
    public Mock<IProfileService> ProfileServiceMock { get; } = MockFactory.CreateProfileService();

    /// <summary>
    ///     Configures the test server: replaces the SQL Server <see cref="AppDatabaseContext" />
    ///     with a SQLite-backed <see cref="SqliteDbContext" /> and swaps all service-layer
    ///     dependencies with Moq stubs.
    /// </summary>
    /// <param name="builder">The web host builder.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // ── Replace SQL Server DbContext with SQLite in-memory ──────────────
            // Remove all registrations that AddDbContext<AppDatabaseContext> created.
            RemoveDbContextRegistrations(services);

            // Register SqliteDbContext (subclass of AppDatabaseContext) so that
            // Program.cs's GetRequiredService<AppDatabaseContext>() returns a
            // SQLite-backed instance.
            services.AddDbContext<AppDatabaseContext, SqliteDbContext>(options =>
                options.UseSqlite(SqliteConnectionString)
                       .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

            // ── Replace all infrastructure services with substitutes ────────────
            ReplaceService(services, JwtServiceMock.Object);
            ReplaceService(services, AuthRepositoryMock.Object);
            ReplaceService(services, LoginServiceMock.Object);
            ReplaceService(services, RegistrationServiceMock.Object);
            ReplaceService(services, PasswordRecoveryServiceMock.Object);
            ReplaceService(services, DashboardServiceMock.Object);
            ReplaceService(services, ProfileServiceMock.Object);
        });
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _keepAliveConnection.Close();
            _keepAliveConnection.Dispose();
        }

        base.Dispose(disposing);
    }

    private static void RemoveDbContextRegistrations(IServiceCollection services)
    {
        // AddDbContext registers DbContextOptions<TContext>, TContext, and several
        // internal services. Removing the options and the context itself is enough
        // to allow the new registration to take over cleanly.
        List<ServiceDescriptor> toRemove = services
            .Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDatabaseContext>) ||
                d.ServiceType == typeof(AppDatabaseContext))
            .ToList();

        foreach (ServiceDescriptor descriptor in toRemove)
        {
            services.Remove(descriptor);
        }
    }

    private static void ReplaceService<TService>(IServiceCollection services, TService implementation)
        where TService : class
    {
        ServiceDescriptor? existing = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(TService));
        if (existing != null)
        {
            services.Remove(existing);
        }

        services.AddScoped(_ => implementation);
    }

    /// <summary>
    ///     Creates the SQLite schema via <c>EnsureCreated()</c> on
    ///     <see cref="SqliteDbContext" /> and pre-populates
    ///     <c>__EFMigrationsHistory</c> so that <c>Database.Migrate()</c> in
    ///     <c>Program.cs</c> finds no pending migrations and returns immediately.
    /// </summary>
    private void InitializeDatabase()
    {
        DbContextOptions<AppDatabaseContext> options =
            new DbContextOptionsBuilder<AppDatabaseContext>()
                .UseSqlite(_keepAliveConnection)
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

        using SqliteDbContext context = new(options);

        // Create all tables from the EF model using SQLite-compatible DDL.
        // SqliteDbContext.OnModelCreating strips GETUTCDATE() defaults so that
        // EnsureCreated() does not produce invalid SQLite expressions.
        context.Database.EnsureCreated();

        // Create the EF migrations history table so Migrate() can read it.
        context.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId"     TEXT NOT NULL,
                "ProductVersion"  TEXT NOT NULL,
                CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
            )
            """);

        // Mark all migrations as applied so Migrate() considers the schema current.
        context.Database.ExecuteSqlRaw("""
            INSERT OR IGNORE INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260426134342_InitialCreate',          '10.0.7'),
                   ('20260501000001_AddTransactionType',     '10.0.7')
            """);
    }
}
