namespace BankingApp.Desktop.ViewModels;

using System;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Profile;
using Enums;
using Services;
using Utilities;
using BankingApp.Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging;

/// <summary>
///     Handles password changes and two-factor authentication settings for the profile area.
/// </summary>
public class SecurityViewModel
{
    private readonly IProfileClientService _profileClientService;
    private readonly ILogger<SecurityViewModel> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SecurityViewModel" /> class.
    /// </summary>
    public SecurityViewModel(IProfileClientService profileClientService, ILogger<SecurityViewModel> logger)
    {
        _profileClientService = profileClientService ?? throw new ArgumentNullException(nameof(profileClientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        State = new ObservableState<ProfileState>(ProfileState.Idle);
    }

    /// <summary>
    ///     Gets the current security workflow state.
    /// </summary>
    public ObservableState<ProfileState> State { get; }

    /// <summary>
    ///     Enables or disables two-factor authentication using the default email flow.
    /// </summary>
    public async Task<bool> SetTwoFactorEnabled(bool enabled)
    {
        return enabled ? await EnableTwoFactor(TwoFactorMethod.Email) : await DisableTwoFactor();
    }

    /// <summary>
    ///     Changes the user's password.
    /// </summary>
    public async Task<(bool Success, string ErrorMessage)> ChangePassword(
        int userId,
        string currentPassword,
        string newPassword,
        string confirmPassword)
    {
        if (!PasswordValidator.MeetsMinimumLength(newPassword))
        {
            return (false, UserMessages.Security.MinimumLengthRequired);
        }

        if (newPassword != confirmPassword)
        {
            return (false, UserMessages.Security.PasswordMismatch);
        }

        State.SetValue(ProfileState.Loading);
        var request = new ChangePasswordRequest(userId, currentPassword, newPassword);
        ErrorOr<Success> result = await _profileClientService.ChangePasswordAsync(request);
        return result.Match(
            _ =>
            {
                State.SetValue(ProfileState.UpdateSuccess);
                return (true, string.Empty);
            },
            errors =>
            {
                _logger.ChangePasswordFailed(errors);
                State.SetValue(ProfileState.Error);
                string message = errors.First().Code == "incorrect_password"
                    ? UserMessages.Security.IncorrectPassword
                    : UserMessages.Security.UnexpectedError;
                return (false, message);
            });
    }

    /// <summary>
    ///     Enables two-factor authentication using the specified method.
    /// </summary>
    public async Task<bool> EnableTwoFactor(TwoFactorMethod method)
    {
        State.SetValue(ProfileState.Loading);
        var request = new EnableTwoFaRequest { Method = method };
        ErrorOr<Success> result = await _profileClientService.Enable2FaAsync(request);
        return result.Match(
            _ =>
            {
                State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                _logger.EnableTwoFactorFailed(errors);
                State.SetValue(ProfileState.Error);
                return false;
            });
    }

    /// <summary>
    ///     Disables two-factor authentication.
    /// </summary>
    public async Task<bool> DisableTwoFactor()
    {
        State.SetValue(ProfileState.Loading);
        ErrorOr<Success> result = await _profileClientService.Disable2FaAsync();
        return result.Match(
            _ =>
            {
                State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                _logger.DisableTwoFactorFailed(errors);
                State.SetValue(ProfileState.Error);
                return false;
            });
    }
}
