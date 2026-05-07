namespace BankingApp.Desktop.Utilities;

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

/// <summary>
///     Maps <see langword="true" /> to <see cref="Visibility.Collapsed" /> and
///     <see langword="false" /> (or non-bool) to <see cref="Visibility.Visible" /> -
///     the inverse of <see cref="BoolToVisibilityConverter" />.
/// </summary>
public partial class InverseBoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    ///     Converts a boolean value to the inverse <see cref="Visibility" />.
    /// </summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">The parameter (unused).</param>
    /// <param name="language">The language (unused).</param>
    /// <returns><see cref="Visibility.Collapsed" /> when <paramref name="value" /> is true; otherwise <see cref="Visibility.Visible" />.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isVisible)
        {
            return isVisible ? Visibility.Collapsed : Visibility.Visible;
        }

        return Visibility.Visible;
    }

    /// <summary>
    ///     Converts a <see cref="Visibility" /> value back to an inverse boolean.
    /// </summary>
    /// <param name="value">The visibility value.</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">The parameter (unused).</param>
    /// <param name="language">The language (unused).</param>
    /// <returns><see langword="true" /> when <paramref name="value" /> is <see cref="Visibility.Collapsed" />.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is Visibility.Collapsed;
    }
}
