namespace BankingApp.Application.DependencyInjection;

using Features.Beneficiaries.Services;
using Features.Billers.Services;
using Features.AccountOverview.Services;
using Features.Forex.Services;
using Features.Authentication.Services;
using Features.PasswordReset.Services;
using Features.UserProfile.Services;
using Features.ForexRateAlerts.Services;
using Features.RecurringPayments.Services;
using Features.UserRegistration.Services;
using Features.Transfers.Services;
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
        services.AddScoped<IUserRegistrationService, UserRegistrationService>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IAccountOverviewService, AccountOverviewService>();
        services.AddScoped<IBillerService, BillerService>();
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<IBeneficiaryService, BeneficiaryService>();
        services.AddScoped<IRecurringPaymentService, RecurringPaymentService>();
        services.AddScoped<IRecurringPaymentProcessingService, RecurringPaymentProcessingService>();
        services.AddScoped<IForexService, ForexService>();
        services.AddScoped<IForexRateAlertService, ForexRateAlertService>();
        return services;
    }
}
