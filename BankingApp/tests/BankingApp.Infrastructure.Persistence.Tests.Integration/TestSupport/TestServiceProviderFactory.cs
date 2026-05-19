namespace BankingApp.Infrastructure.Persistence.Tests.Integration.TestSupport;

using Infrastructure.Persistence.DependencyInjection;

public sealed class TestServiceProviderFactory(string connectionString)
{
    public IServiceProvider Create()
    {
        ServiceCollection services = new();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:BankingAppDb"] = connectionString,
                ["Jwt:Secret"] = "integration-tests-jwt-secret",
                ["Otp:Secret"] = "integration-tests-otp-secret",
                ["Email:SmtpHost"] = "localhost",
                ["Email:SmtpPort"] = "2525",
                ["Email:SmtpUser"] = "integration-user",
                ["Email:SmtpPass"] = "integration-pass",
                ["Email:FromAddress"] = "integration@bankingapp.local"
            })
            .Build();

        services.AddSingleton(Mock.Of<IPublisher>());
        services.AddPersistenceInfrastructure(configuration);

        return services.BuildServiceProvider(validateScopes: true);
    }
}
