// <copyright file="BankAppWebFactory.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Dashboard;
using BankApp.Application.Services.Login;
using BankApp.Application.Services.PasswordRecovery;
using BankApp.Application.Services.Profile;
using BankApp.Application.Services.Registration;
using BankApp.Application.Services.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankApp.Api.Tests.Integration.Infrastructure;

/// <summary>
///     A custom <see cref="WebApplicationFactory{TEntryPoint}" /> that replaces all
///     infrastructure services with Moq stubs so integration tests can exercise the
///     full HTTP pipeline without a database or external dependencies.
/// </summary>
public class BankAppWebFactory : WebApplicationFactory<Program>
{
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
    ///     Configures the test server so the middleware pipeline runs end-to-end
    ///     but all service-layer dependencies are replaced with substitutes.
    /// </summary>
    /// <param name="builder">The web host builder.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, configurationBuilder) =>
        {
            // Provide the configuration values that AddInfrastructure requires so it
            // does not throw during startup. The real services are replaced below.
            configurationBuilder.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:BankingAppDb"] = "Server=fake;Database=fake;",
                    ["Jwt:Secret"] = "integration-test-secret-that-is-long-enough-for-hmac",
                    ["Otp:Secret"] = "integration-test-otp-secret-placeholder",
                });
        });

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