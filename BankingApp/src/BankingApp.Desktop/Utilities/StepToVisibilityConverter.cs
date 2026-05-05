// <copyright file="StepToVisibilityConverter.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the StepToVisibilityConverter class.
// </summary>

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace BankingApp.Desktop.Utilities;

/// <summary>
///     Returns <see cref="Visibility.Visible" /> when the current wizard step matches
///     the target step number supplied as a string converter parameter; collapses otherwise.
/// </summary>
public partial class StepToVisibilityConverter : IValueConverter
{
    /// <summary>
    ///     Converts an integer step value to <see cref="Visibility" /> by comparing it with
    ///     the target step number provided as a string in <paramref name="parameter" />.
    /// </summary>
    /// <param name="value">The current step number.</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">The target step number as a string.</param>
    /// <param name="language">The language (unused).</param>
    /// <returns><see cref="Visibility.Visible" /> if the step matches; otherwise <see cref="Visibility.Collapsed" />.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int currentStepNumber
            && parameter is string targetStepText
            && int.TryParse(targetStepText, out int targetStepNumber))
        {
            return currentStepNumber == targetStepNumber ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    /// <summary>
    ///     Not supported - this converter is one-way only.
    /// </summary>
    /// <param name="value">The value (unused).</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">The parameter (unused).</param>
    /// <param name="language">The language (unused).</param>
    /// <returns>Nothing - always throws.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
