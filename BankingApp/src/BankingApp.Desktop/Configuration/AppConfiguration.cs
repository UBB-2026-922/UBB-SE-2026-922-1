namespace BankingApp.Desktop.Configuration;

using System;
using Microsoft.Extensions.Configuration;

internal static class AppConfiguration
{
    public static IConfigurationRoot Build()
    {
        string environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
