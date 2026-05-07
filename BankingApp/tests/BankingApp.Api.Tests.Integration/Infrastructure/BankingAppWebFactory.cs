using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.Beneficiary;
using BankingApp.Application.Services.Billers;
using BankingApp.Application.Services.Dashboard;
using BankingApp.Application.Services.Login;
using BankingApp.Application.Services.PasswordRecovery;
using BankingApp.Application.Services.Profile;
using BankingApp.Application.Services.Registration;
using BankingApp.Application.Services.Security;
using BankingApp.Infrastructure.DataAccess;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace BankingApp.Api.Tests.Integration.Infrastructure;

/// <summary>
///     A custom <see cref="WebApplicationFactory{TEntryPoint}" /> that:
///     <list type="bullet">
///         <item>Replaces all infrastructure services with Moq stubs.</item>
///         <item>Runs the API in the Testing environment so startup does not apply database migrations.</item>
///     </list>
/// </summary>
public class BankingAppWebFactory : WebApplicationFactory<Program>
{
    private const string FallbackConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=BankingAppApiTests;Trusted_Connection=True;TrustServerCertificate=True;";
    private readonly string _testConnectionString;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BankingAppWebFactory" /> class.
    /// </summary>
    public BankingAppWebFactory()
    {
        _testConnectionString = Environment.GetEnvironmentVariable("MSSQL_CONNECTION_STRING")
            ?? FallbackConnectionString;

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("Database__ApplyMigrations", "false");
        Environment.SetEnvironmentVariable("ConnectionStrings__BankingAppDb", _testConnectionString);
        Environment.SetEnvironmentVariable("Jwt__Secret", "integration-test-secret-that-is-long-enough-for-hmac");
        Environment.SetEnvironmentVariable("Otp__Secret", "integration-test-otp-secret-placeholder");
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
    ///     Gets the mock beneficiary service.
    /// </summary>
    public Mock<IBeneficiaryService> BeneficiaryServiceMock { get; } = MockFactory.CreateBeneficiaryService();

    /// <summary>
    ///     Gets the mock biller service.
    /// </summary>
    public Mock<IBillerService> BillerServiceMock { get; } = MockFactory.CreateBillerService();

    /// <summary>
    ///     Configures the test server by replacing service-layer dependencies with Moq stubs.
    /// </summary>
    /// <param name="builder">The web host builder.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(WebHostDefaults.ApplicationKey, typeof(Program).Assembly.GetName().Name);
        builder.UseEnvironment("Testing");
        builder.UseDefaultServiceProvider(options =>
        {
            options.ValidateScopes = false;
            options.ValidateOnBuild = false;
        });

        builder.ConfigureTestServices(services =>
        {
            // Ensure controllers from the API assembly are discovered
            services.AddControllers().AddApplicationPart(typeof(Program).Assembly);

            // Remove real infrastructure registrations and replace with substitutes.
            ReplaceService<IJsonWebTokenService>(services, JwtServiceMock.Object);
            ReplaceService<IAuthRepository>(services, AuthRepositoryMock.Object);
            ReplaceService<ILoginService>(services, LoginServiceMock.Object);
            ReplaceService<IRegistrationService>(services, RegistrationServiceMock.Object);
            ReplaceService<IPasswordRecoveryService>(services, PasswordRecoveryServiceMock.Object);
            ReplaceService<IDashboardService>(services, DashboardServiceMock.Object);
            ReplaceService<IProfileService>(services, ProfileServiceMock.Object);
            ReplaceService<IBeneficiaryService>(services, BeneficiaryServiceMock.Object);
            ReplaceService<IBillerService>(services, BillerServiceMock.Object);
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        EnsureDatabaseExists(_testConnectionString);

        IHost host = base.CreateHost(builder);

        using IServiceScope scope = host.Services.CreateScope();
        AppDatabaseContext databaseContext = scope.ServiceProvider.GetRequiredService<AppDatabaseContext>();
        databaseContext.Database.Migrate();

        return host;
    }

    private static void EnsureDatabaseExists(string connectionString)
    {
        var connectionBuilder = new SqlConnectionStringBuilder(connectionString);
        string? databaseName = connectionBuilder.InitialCatalog;
        if (string.IsNullOrWhiteSpace(databaseName) ||
            string.Equals(databaseName, "master", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string escapedDatabaseName = databaseName.Replace("]", "]]", StringComparison.Ordinal);
        connectionBuilder.InitialCatalog = "master";

        using var masterConnection = new SqlConnection(connectionBuilder.ConnectionString);
        masterConnection.Open();

        using SqlCommand createDatabaseCommand = masterConnection.CreateCommand();
        createDatabaseCommand.CommandText = $"IF DB_ID(@databaseName) IS NULL CREATE DATABASE [{escapedDatabaseName}]";
        createDatabaseCommand.Parameters.AddWithValue("@databaseName", databaseName);
        createDatabaseCommand.ExecuteNonQuery();
    }

    private static void ReplaceService<TService>(IServiceCollection services, TService implementation)
        where TService : class
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(TService)).ToList();
        foreach (ServiceDescriptor descriptor in descriptors)
        {
            services.Remove(descriptor);
        }

        services.AddSingleton(_ => implementation);
    }
}
