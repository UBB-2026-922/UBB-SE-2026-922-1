// <copyright file="ObservableState.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the ObservableState class.
// </summary>

using System.Collections.Generic;

namespace BankingApp.Desktop.Utilities;

/// <summary>
///     Simple observable state class that allows observers to
///     subscribe and be notified when the state changes.
/// </summary>
/// <typeparam name="T">The type of the observable state value.</typeparam>
/// <param name="value">The initial state value.</param>
public class ObservableState<T>(T value)
{
    private readonly List<IStateObserver<T>> _observers =
    [
    ];

    /// <summary>
    ///     Gets the current value of the state.
    ///     Observers will be notified whenever this value changes via the SetValue method.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public T Value { get; private set; } = value;

    /// <summary>
    ///     Sets a new value for the state and notifies all subscribed observers of the change.
    /// </summary>
    /// <param name="value">The new state value to apply.</param>
    public void SetValue(T value)
    {
        Value = value;
        NotifyObservers();
    }

    /// <summary>
    ///     Adds an observer to the list of observers that will be notified when the state changes.
    /// </summary>
    /// <param name="observer">The observer to add.</param>
    public void AddObserver(IStateObserver<T> observer)
    {
        _observers.Add(observer);
    }

    /// <summary>
    ///     Removes an observer from the list of _observers.
    ///     The removed observer will no longer receive notifications when the state changes.
    /// </summary>
    /// <param name="observer">The observer to remove.</param>
    public void RemoveObserver(IStateObserver<T> observer)
    {
        _observers.Remove(observer);
    }

    private void NotifyObservers()
    {
        foreach (IStateObserver<T> observer in _observers)
        {
            observer.Update(Value);
        }
    }
}