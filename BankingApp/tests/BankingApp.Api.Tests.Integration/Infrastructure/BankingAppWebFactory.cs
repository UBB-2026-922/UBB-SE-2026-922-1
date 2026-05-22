using BankingApp.Api.HostedServices;
using BankingApp.Application.Common.Security;
using BankingApp.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace BankingApp.Api.Tests.Integration.Infrastructure;

public class BankingAppWebFactory : WebApplicationFactory<Program>
{
    private const string TestConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=BankingAppApiTests;Trusted_Connection=True;TrustServerCertificate=True;";

    public BankingAppWebFactory()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("Database__ApplyMigrations", "false");
        Environment.SetEnvironmentVariable("ConnectionStrings__BankingAppDb", TestConnectionString);
        Environment.SetEnvironmentVariable("Jwt__Secret", "integration-test-secret-that-is-long-enough-for-hmac");
    }

    public Mock<ISender> SenderMock { get; } = new();

    public Mock<IJsonWebTokenService> JwtServiceMock { get; } = new();

    public Mock<IIdentityRepository> IdentityRepositoryMock { get; } = new();

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
            services.AddControllers().AddApplicationPart(typeof(Program).Assembly);

            RemoveHostedService<FinanceBackgroundService>(services);
            ReplaceService(services, SenderMock.Object);
            ReplaceService(services, JwtServiceMock.Object);
            ReplaceService(services, IdentityRepositoryMock.Object);

            services.AddAuthentication("IntegrationTest")
                .AddScheme<AuthenticationSchemeOptions, PassThroughAuthHandler>("IntegrationTest", _ => { });
        });
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

    private static void RemoveHostedService<THostedService>(IServiceCollection services)
        where THostedService : class, IHostedService
    {
        var descriptors = services
            .Where(descriptor => descriptor.ServiceType == typeof(IHostedService) &&
                                 descriptor.ImplementationType == typeof(THostedService))
            .ToList();

        foreach (ServiceDescriptor descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
