// <copyright file="AppNavigationService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the AppNavigationService class.
// </summary>

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace BankingApp.Desktop.Master;

/// <summary>
///     Provides navigation services for the application, enabling switching between pages and managing navigation
///     history.
/// </summary>
/// <remarks>
///     Pages are resolved through the DI container so that their constructor dependencies
///     (e.g. view models) are injected automatically. Navigation is performed by setting
///     the Frame.Content property directly rather than calling <see cref="Frame.Navigate(Type)" />,
///     because the latter instantiates pages via reflection and bypasses the container.
/// </remarks>
public class AppNavigationService : IAppNavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private Frame? _contentFrame;
    private Frame? _frame;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AppNavigationService" /> class.
    /// </summary>
    /// <param name="serviceProvider">
    ///     The DI container used to resolve page instances with their injected dependencies.
    /// </param>
    /// <returns>The result of the operation.</returns>
    public AppNavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    /// <param name="newFrame">The newFrame value.</param>
    public void SetFrame(Frame newFrame)
    {
        _frame = newFrame;
    }

    /// <inheritdoc />
    /// <param name="newFrame">The newFrame value.</param>
    public void SetContentFrame(Frame newFrame)
    {
        _contentFrame = newFrame;
    }

    /// <inheritdoc />
    public void NavigateTo<TPage>()
        where TPage : class
    {
        var page = _serviceProvider.GetRequiredService<TPage>();
        _frame!.Content = page;
    }

    /// <inheritdoc />
    public void NavigateToContent<TPage>()
        where TPage : class
    {
        var page = _serviceProvider.GetRequiredService<TPage>();
        _contentFrame!.Content = page;
    }
}