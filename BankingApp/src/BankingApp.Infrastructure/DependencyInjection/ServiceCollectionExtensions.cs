namespace BankingApp.Infrastructure.DependencyInjection;

using BankingApp.Application.Common.Contracts;
using BankingApp.Application.Common.Contracts.Notifications;
using BankingApp.Application.Common.Contracts.Security;
using BankingApp.Application.Common.Utilities;
using BankingApp.Domain.Repositories;
using BankingApp.Infrastructure.Caching;
using BankingApp.Infrastructure.Common.Clock;
using BankingApp.Infrastructure.Common.Notifications;
using BankingApp.Infrastructure.Common.Security;
using BankingApp.Infrastructure.ExchangeRates;
using BankingApp.Infrastructure.Persistence;
using BankingApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("BankingAppDb")
                                  ?? throw new InvalidOperationException(
                                      "Configuration value 'ConnectionStrings:BankingAppDb' is missing.");
        string jwtSecret = configuration["Jwt:Secret"]
                           ?? throw new InvalidOperationException("Configuration value 'Jwt:Secret' is missing.");
        string otpSecret = configuration["Otp:Secret"]
                           ?? throw new InvalidOperationException("Configuration value 'Otp:Secret' is missing.");

        services.AddMemoryCache();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString)
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IIdentityRepository, IdentityRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
        services.AddScoped<IBillerRepository, BillerRepository>();
        services.AddScoped<IBillPaymentRepository, BillPaymentRepository>();
        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<IForexRepository, ForexTransactionRepository>();
        services.AddScoped<IRateAlertRepository, RateAlertRepository>();
        services.AddScoped<IRecurringPaymentRepository, RecurringPaymentRepository>();
        services.AddScoped<ISavedBillerRepository, SavedBillerRepository>();

        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddSingleton<IOtpAttemptTracker, OtpAttemptTracker>();
        services.AddSingleton<IOtpService>(_ => new OtpService(otpSecret));
        services.AddScoped<IHashService, HashService>();
        services.AddScoped<IJsonWebTokenService>(_ => new JsonWebTokenService(jwtSecret));
        services.AddScoped<IEmailService, EmailService>();
        services.AddSingleton<ILockedRateCache, MemoryLockedRateCache>();
        services.AddSingleton<IExchangeRateService, ConfigurationExchangeRateService>();

        return services;
    }
}
