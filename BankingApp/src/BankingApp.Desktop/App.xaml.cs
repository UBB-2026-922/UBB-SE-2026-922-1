// <copyright file="App.xaml.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the App class.
// </summary>

using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
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
    private static string _logDirectory = string.Empty;
    private Window? _window;

    /// <summary>
    ///     Initializes a new instance of the <see cref="App" /> class
    ///     and builds the dependency injection container.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public App()
    {
        ConfigureLogging();
        RegisterGlobalExceptionLogging();
        Log.Information("Desktop app starting. BaseDirectory={BaseDirectory}, LogDirectory={LogDirectory}.",
            AppContext.BaseDirectory,
            _logDirectory);
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
    /// <param name="args">
    ///     Contains information about the launch request and process, such as the
    ///     activation kind and previous execution state.
    /// </param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Log.Information("Desktop app launched.");
        IAppNavigationService navigationService = Services.GetRequiredService<IAppNavigationService>();
        _window = new MainWindow(navigationService);
        _window.Activate();
    }

    private static void ConfigureLogging()
    {
        _logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BankingApp",
            "Logs");
        Directory.CreateDirectory(_logDirectory);
        const string loggingFileFormat = "bankingapp-client-.log";
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            // Writes to the Visual Studio Output window during development.
            .WriteTo.Debug(formatProvider: CultureInfo.InvariantCulture)
            // Writes to a daily rolling file outside the repository.
            // Log path: %LocalAppData%\BankingApp\Logs\bankingapp-client-YYYYMMDD.log
            .WriteTo.File(
                Path.Combine(_logDirectory, loggingFileFormat),
                formatProvider: CultureInfo.InvariantCulture,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: RetainedLoggingFileCountLimit)
            .CreateLogger();
    }

    private void RegisterGlobalExceptionLogging()
    {
        UnhandledException += OnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Log.Fatal(e.Exception, "UI thread unhandled exception.");
        Log.CloseAndFlush();
    }

    private static void OnCurrentDomainUnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
            Log.Fatal(exception, "AppDomain unhandled exception. IsTerminating={IsTerminating}.", e.IsTerminating);
        else
            Log.Fatal("AppDomain unhandled non-exception object. IsTerminating={IsTerminating}.", e.IsTerminating);

        Log.CloseAndFlush();
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unobserved task exception.");
        Log.CloseAndFlush();
    }
}
