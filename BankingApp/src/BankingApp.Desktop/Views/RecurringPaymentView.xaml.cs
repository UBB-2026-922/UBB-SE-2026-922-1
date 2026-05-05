// <copyright file="RecurringPaymentView.xaml.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains code for RecurringPaymentView.xaml.
// </summary>

using System;
using BankingApp.Application.DTOs.TeamB;
using BankingApp.Desktop.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BankingApp.Desktop.Views;

/// <summary>
///     Displays the recurring payment management screen.
/// </summary>
public sealed partial class RecurringPaymentView : Page
{
    private const decimal ZeroAmount = 0m;
    private readonly RecurringPaymentViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RecurringPaymentView" /> class.
    /// </summary>
    /// <param name="viewModel">The view model.</param>
    public RecurringPaymentView(RecurringPaymentViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        StartDatePicker.Date = DateTimeOffset.Now;
        EndDatePicker.Date = DateTimeOffset.Now;
    }

    /// <inheritdoc/>
    protected override async void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
    {
        base.OnNavigatedTo(navigationEventArgs);
        await _viewModel.LoadAsync();
    }

    private void AmountNumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        _viewModel.Amount = double.IsNaN(sender.Value) ? ZeroAmount : (decimal)sender.Value;
    }

    private void StartDatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs args)
    {
        if (sender is DatePicker picker)
        {
            _viewModel.StartDate = picker.Date.DateTime.Date;
        }
    }

    private void EndDatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs args)
    {
        if (sender is DatePicker picker)
        {
            _viewModel.EndDate = picker.Date.DateTime.Date;
        }
    }

    private async void CreateButton_Click(object sender, RoutedEventArgs args)
    {
        await _viewModel.CreateAsync();
    }

    private void PauseButton_Click(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: RecurringPaymentDto payment })
        {
            _ = _viewModel.PauseAsync(payment);
        }
    }

    private void ResumeButton_Click(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: RecurringPaymentDto payment })
        {
            _ = _viewModel.ResumeAsync(payment);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: RecurringPaymentDto payment })
        {
            _ = _viewModel.CancelAsync(payment);
        }
    }
}
