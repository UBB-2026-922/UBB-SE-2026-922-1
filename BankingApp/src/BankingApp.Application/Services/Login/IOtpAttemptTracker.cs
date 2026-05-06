namespace BankingApp.Application.Services.Login;

/// <summary>
///     Tracks consecutive failed OTP verification attempts per user.
/// </summary>
public interface IOtpAttemptTracker
{
    /// <summary>
    ///     Records a failed OTP attempt for the user and returns the new total.
    /// </summary>
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public int RecordFailure(int userId);

    /// <summary>
    ///     Clears the failure counter for the user (on success, resend, or max exceeded).
    /// </summary>
    /// <param name="userId">The userId value.</param>
    public void Reset(int userId);
}
