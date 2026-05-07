namespace BankingApp.Infrastructure.DependencyInjection;


using BankingApp.Application.Features.AccountOverview.Repositories;
using BankingApp.Application.Features.Authentication.Repositories;
using BankingApp.Application.Features.Beneficiaries.Repositories;
using BankingApp.Application.Features.Billers.Repositories;
using BankingApp.Application.Features.BillPayments.Repositories;
using BankingApp.Application.Features.BillPayments.Services;
using BankingApp.Application.Features.Authentication.Services;
using BankingApp.Application.Common.Notifications;
using BankingApp.Application.Common.Security;
using BankingApp.Application.Common.Utilities;
using BankingApp.Application.Features.Forex.Repositories;
using BankingApp.Application.Features.ForexRateAlerts.Repositories;
using BankingApp.Application.Features.RecurringPayments.Repositories;
using BankingApp.Application.Features.Transfers.Repositories;
using BankingApp.Application.Features.UserProfile.Repositories;
using BankingApp.Infrastructure.DataAccess;
using BankingApp.Infrastructure.DataAccess.Implementations;
using BankingApp.Infrastructure.DataAccess.Interfaces;
using BankingApp.Infrastructure.Repositories.Authentication;
using BankingApp.Infrastructure.Repositories.Beneficiaries;
using BankingApp.Infrastructure.Repositories.Billers;
using BankingApp.Infrastructure.Repositories.BillPayments;
using BankingApp.Infrastructure.Repositories.Forex;
using BankingApp.Infrastructure.Repositories.ForexRateAlerts;
using BankingApp.Infrastructure.Repositories.RecurringPayments;
using BankingApp.Infrastructure.Repositories.Transfers;
using BankingApp.Infrastructure.Repositories.UserProfile;
using BankingApp.Infrastructure.Common.Notifications;
using BankingApp.Infrastructure.Common.Security;
using Features.AccountOverview;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

/// <summary>
///     Provides extension methods for registering infrastructure services with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    private static readonly Lock _typeHandlerLock = new();

    /// <summary>
    ///     Registers all infrastructure services, data access components, and repositories with the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration used to resolve connection strings and secrets.</param>
    /// <returns>The same <see cref="IServiceCollection" /> instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the <c>BankingAppDb</c> connection string,
    ///     <c>Jwt:Secret</c> or <c>Otp:Secret</c> configuration value is missing.
    /// </exception>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("BankingAppDb")
                                  ?? throw new InvalidOperationException(
                                      "Configuration value 'ConnectionStrings:BankingAppDb' is missing.");
        string jwtSecret = configuration["Jwt:Secret"]
                           ?? throw new InvalidOperationException("Configuration value 'Jwt:Secret' is missing.");
        string otpSecret = configuration["Otp:Secret"]
                           ?? throw new InvalidOperationException("Configuration value 'Otp:Secret' is missing.");

        services.AddDbContext<AppDatabaseContext>(options =>
            options.UseSqlServer(connectionString)
                   .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));
        services.AddScoped<IUserDataAccess, UserDataAccess>();
        services.AddScoped<ISessionDataAccess, SessionDataAccess>();
        services.AddScoped<IPasswordResetTokenDataAccess, PasswordResetTokenDataAccess>();
        services.AddScoped<INotificationPreferenceDataAccess, NotificationPreferenceDataAccess>();
        services.AddScoped<IAccountDataAccess, AccountDataAccess>();
        services.AddScoped<ICardDataAccess, CardDataAccess>();
        services.AddScoped<ITransactionDataAccess, TransactionDataAccess>();
        services.AddScoped<INotificationDataAccess, NotificationDataAccess>();
        services.AddScoped<IHashService, HashService>();
        services.AddScoped<IJsonWebTokenService>(_ => new JsonWebTokenService(jwtSecret));
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAccountOverviewRepository, AccountOverviewRepository>();
        services.AddScoped<IBillPaymentRepository, BillPaymentRepository>();
        services.AddScoped<
            IBillPaymentService,
            BillPaymentService>();
        services.AddScoped<IBillerDataAccess, BillerDataAccess>();
        services.AddScoped<ISavedBillerDataAccess, SavedBillerDataAccess>();
        services.AddScoped<IBillerRepository, BillerRepository>();
        services.AddScoped<ITransferDataAccess, TransferDataAccess>();
        services.AddScoped<IRecurringPaymentRepository, RecurringPaymentRepository>();
        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddSingleton<IOtpAttemptTracker, OtpAttemptTracker>();
        services.AddSingleton<IOtpService, OtpService>(_ => new OtpService(otpSecret));

        // These registrations wire the interfaces defined in the Application layer to the
        // repository implementations in the Infrastructure layer.
        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
        services.AddScoped<IForexRepository, ForexRepository>();
        services.AddScoped<IForexRateAlertRepository, ForexRateAlertRepository>();

        return services;
    }
}
