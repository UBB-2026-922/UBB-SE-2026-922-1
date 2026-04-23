// <copyright file="IStateObserver.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the IStateObserver interface.
// </summary>

namespace BankingApp.Desktop.Utilities;

/// <summary>
///     Simple observer interface for observing changes in a state of type T.
/// </summary>
/// <typeparam name="T">The type of the observed state.</typeparam>
public interface IStateObserver<in T>
{
    /// <summary>
    ///     Updates the observer with a new value of type T,
    ///     allowing it to react to changes in the observed state.
    /// </summary>
    /// <param name="value">The new state value.</param>
    void Update(T value);
}