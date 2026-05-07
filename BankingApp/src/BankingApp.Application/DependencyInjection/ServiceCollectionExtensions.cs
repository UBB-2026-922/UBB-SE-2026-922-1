namespace BankingApp.Application.DependencyInjection;

using Services.Beneficiary;
using Services.Billers;
using Services.Dashboard;
using Services.Exchange;
using Services.Login;
using Services.PasswordRecovery;
using Services.Profile;
using Services.RateAlerts;
using Services.RecurringPayments;
using Services.Registration;
using Services.Transfers;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Provides extension methods for registering application-layer services with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers application use cases and orchestration services with the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection" /> instance for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IPasswordRecoveryService, PasswordRecoveryService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IBillerService, BillerService>();
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<IBeneficiaryService, BeneficiaryService>();
        services.AddScoped<IRecurringPaymentService, RecurringPaymentService>();
        services.AddScoped<IRecurringPaymentProcessingService, RecurringPaymentProcessingService>();
        services.AddScoped<IExchangeService, ExchangeService>();
        services.AddScoped<IRateAlertService, RateAlertService>();
        return services;
    }
}
