// <copyright file="App.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the App class.
// </summary>

using System;
using System.IO;
using BankingApp.Desktop.DependencyInjection;
using BankingApp.Desktop.Master;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Serilog;

namespace BankingApp.Desktop;

/// <summary>
///     Composition root for the client application.
/// </summary>
public partial class App
{
    private const int RetainedLoggingFileCountLimit = 14;
    private Window? _window;

    /// <summary>
    ///     Initializes a new instance of the <see cref="App" /> class
    ///     and builds the dependency injection container.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public App()
    {
        ConfigureLogging();
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false)
            // appsettings.Local.json is `.gitignore`
            // and only exists in dev environments.
            // It overrides appsettings.json locally.
            .AddJsonFile("appsettings.Local.json", true)
            // Environment variables are the final override layer for CI/Prod builds.
            .AddEnvironmentVariables()
            .Build();
        var serviceCollection = new ServiceCollection();
        // AddLogging registers ILoggerFactory and ILogger<T> in the container.
        // AddSerilog bridges the MEL abstraction to the Serilog backend configured above.
        // dispose: true ensures Serilog flushes when the container is disposed.
        serviceCollection.AddLogging(logging => logging.AddSerilog(dispose: true));
        serviceCollection.AddClientServices(configuration);
        Services = serviceCollection.BuildServiceProvider();
        InitializeComponent();
    }

    /// <summary>
    ///     Gets the application-wide DI container.
    ///     Only resolve services at the composition root boundary (that is in <see cref="OnLaunched" />).
    ///     All other classes must receive their dependencies via constructor injection.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    private IServiceProvider Services { get; }

    /// <summary>
    ///     Invoked when the application is launched. Resolves the navigation service,
    ///     creates the main window and activates it.
    /// </summary>
    /// <param name="arguments">
    ///     Contains information about the launch request and process, such as the
    ///     activation kind and previous execution state.
    /// </param>
    protected override void OnLaunched(LaunchActivatedEventArgs arguments)
    {
        var navigationService = Services.GetRequiredService<IAppNavigationService>();
        _window = new MainWindow(navigationService);
        _window.Activate();
    }

    private static void ConfigureLogging()
    {
        string logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BankingApp",
            "Logs");
        const string loggingFileFormat = "bankingapp-client-.log";
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            // Writes to the Visual Studio Output window during development.
            .WriteTo.Debug()
            // Writes to a daily rolling file outside the repository.
            // Log path: %LocalAppData%\BankingApp\Logs\bankingapp-client-YYYYMMDD.log
            .WriteTo.File(
                Path.Combine(logDirectory, loggingFileFormat),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: RetainedLoggingFileCountLimit)
            .CreateLogger();
    }
}