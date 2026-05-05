// <copyright file="BankingAppWebFactory.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.Dashboard;
using BankingApp.Application.Services.Login;
using BankingApp.Application.Services.PasswordRecovery;
using BankingApp.Application.Services.Profile;
using BankingApp.Application.Services.Registration;
using BankingApp.Application.Services.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

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
    private const string TestConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=BankingAppApiTests;Trusted_Connection=True;TrustServerCertificate=True;";

    /// <summary>
    ///     Initializes a new instance of the <see cref="BankingAppWebFactory" /> class.
    /// </summary>
    public BankingAppWebFactory()
    {
        // These environment variables must be set before the host is built so that
        // AddInfrastructure does not throw and Program.cs does not run migrations.
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("Database__ApplyMigrations", "false");
        Environment.SetEnvironmentVariable("ConnectionStrings__BankingAppDb", TestConnectionString);
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
    ///     Configures the test server by replacing service-layer dependencies with Moq stubs.
    /// </summary>
    /// <param name="builder">The web host builder.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove real infrastructure registrations and replace with substitutes.
            ReplaceService(services, JwtServiceMock.Object);
            ReplaceService(services, AuthRepositoryMock.Object);
            ReplaceService(services, LoginServiceMock.Object);
            ReplaceService(services, RegistrationServiceMock.Object);
            ReplaceService(services, PasswordRecoveryServiceMock.Object);
            ReplaceService(services, DashboardServiceMock.Object);
            ReplaceService(services, ProfileServiceMock.Object);
        });
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
}
