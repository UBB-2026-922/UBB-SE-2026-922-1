namespace BankingApp.Desktop.Utilities;

using System;

/// <summary>
///     Abstracts a repeating one-second timer used for countdown logic.
///     Injecting this interface instead of <see cref="Microsoft.UI.Xaml.DispatcherTimer" /> directly
///     allows ViewModels that own countdown state to remain free of WinUI references and
///     therefore fully testable without a UI thread.
/// </summary>
public interface ICountdownTimer
{
    /// <summary>
    ///     Raised approximately once per second while the timer is running.
    /// </summary>
    public event EventHandler? Tick;

    /// <summary>
    ///     Starts the countdown. Subsequent <see cref="Tick" /> events begin firing
    ///     at one-second intervals until <see cref="StopTimer" /> is called.
    /// </summary>
    public void Start();

    /// <summary>
    ///     Stops the countdown. No further <see cref="Tick" /> events are raised
    ///     until <see cref="Start" /> is called again.
    /// </summary>
    public void StopTimer();
}
