namespace BankingApp.Desktop.Configuration;

using System;
using Microsoft.Extensions.Configuration;

internal static class AppConfiguration
{
    public static IConfigurationRoot Build()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
