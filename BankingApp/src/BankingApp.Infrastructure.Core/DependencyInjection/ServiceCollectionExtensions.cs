namespace BankingApp.Infrastructure.Core.DependencyInjection;

using Application;
using Application.Features.Forex.Services;
using Caching;
using ExchangeRates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ILockedRateCache, MemoryLockedRateCache>();
        services.AddSingleton<IExchangeRateService, ConfigurationExchangeRateService>();

        return services;
    }
}
