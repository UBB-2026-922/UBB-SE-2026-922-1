namespace BankingApp.Desktop.Utilities;

using System;
using System.Linq;
using System.Threading.Tasks;
using Enums;
using ErrorOr;
using Services;

/// <summary>
///     Implements <see cref="IPasswordRecoveryManager" /> by delegating auth work
///     to <see cref="IAuthClientService" /> and managing resend-throttling via <see cref="ISystemClock" />.
/// </summary>
public class PasswordRecoveryManager : IPasswordRecoveryManager
{
    private const int ResendCooldownSeconds = 60;
    private const int NoSecondsRemaining = 0;
    private readonly IAuthClientService _authClientService;
    private readonly ISystemClock _clock;
    private DateTime? _lastCodeRequestedAt;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PasswordRecoveryManager" /> class.
    /// </summary>
    /// <param name="authClientService">The desktop auth service.</param>
    /// <param name="clock">The system clock abstraction used for throttle calculations.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    /// <returns>The result of the operation.</returns>
    public PasswordRecoveryManager(IAuthClientService authClientService, ISystemClock clock)
    {
        _authClientService = authClientService ?? throw new ArgumentNullException(nameof(authClientService));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <inheritdoc />
    public bool CanResendCode
    {
        get
        {
            if (_lastCodeRequestedAt is null)
            {
                return true;
            }

            return (_clock.UtcNow - _lastCodeRequestedAt.Value).TotalSeconds >= ResendCooldownSeconds;
        }
    }

    /// <inheritdoc />
    public int SecondsUntilResendAllowed
    {
        get
        {
            if (_lastCodeRequestedAt is null)
            {
                return NoSecondsRemaining;
            }

            double elapsed = (_clock.UtcNow - _lastCodeRequestedAt.Value).TotalSeconds;
            double remaining = ResendCooldownSeconds - elapsed;
            return remaining > 0 ? (int)Math.Ceiling(remaining) : NoSecondsRemaining;
        }
    }

    /// <inheritdoc />
    /// <param name="email">The email value.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<ForgotPasswordState> RequestCodeAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return ForgotPasswordState.Error;
        }

        if (!CanResendCode)
        {
            return ForgotPasswordState.EmailSent;
        }

        ErrorOr<Success> result = await _authClientService.RequestPasswordResetAsync(email);
        return result.Match(
            _ =>
            {
                _lastCodeRequestedAt = _clock.UtcNow;
                return ForgotPasswordState.EmailSent;
            },
            _ => ForgotPasswordState.Error);
    }

    /// <inheritdoc />
    /// <param name="token">The token value.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<ForgotPasswordState> VerifyTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return ForgotPasswordState.Error;
        }

        ErrorOr<Success> result = await _authClientService.VerifyResetTokenAsync(token);
        return result.Match(
            _ => ForgotPasswordState.TokenValid,
            errors => MapError(errors.First()));
    }

    /// <inheritdoc />
    /// <param name="token">The token value.</param>
    /// <param name="newPassword">The newPassword value.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<ForgotPasswordState> ResetPasswordAsync(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
        {
            return ForgotPasswordState.Error;
        }

        ErrorOr<Success> result = await _authClientService.ResetPasswordAsync(token, newPassword);
        return result.Match(
            _ => ForgotPasswordState.PasswordResetSuccess,
            errors => MapError(errors.First()));
    }

    /// <inheritdoc />
    /// <param name="password">The password value.</param>
    /// <returns>The result of the operation.</returns>
    public bool IsPasswordValid(string password)
    {
        return _authClientService.IsPasswordValid(password);
    }

    private static ForgotPasswordState MapError(Error error)
    {
        return error.Code switch
        {
            "token_expired" => ForgotPasswordState.TokenExpired,
            "token_already_used" => ForgotPasswordState.TokenAlreadyUsed,
            _ => ForgotPasswordState.Error
        };
    }
}
