namespace BankingApp.Desktop.Utilities;

/// <summary>
///     Defines a contract for notifying subscribers when a property value changes.
/// </summary>
internal interface IPropertyChangeNotifier
{
    /// <summary>
    ///     Raises a property-changed notification for the given property name.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    public void OnPropertyChanged(string propertyName);
}
