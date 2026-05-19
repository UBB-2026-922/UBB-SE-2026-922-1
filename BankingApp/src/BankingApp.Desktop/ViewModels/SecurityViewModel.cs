namespace BankingApp.Desktop.ViewModels;

using System;
using System.Linq;
using System.Threading.Tasks;
using Enums;
using BankingApp.Domain.Enums;
using Contracts.Features.UserProfile.Dtos;
using Contracts.Features.UserProfile.Services;
using ErrorOr;
using Logging;
using Microsoft.Extensions.Logging;
using Utilities;
using DesktopLogMessages = Logging.DesktopLogMessages;

/// <summary>Handles password changes and two-factor authentication settings for the profile area.</summary>
public partial class SecurityViewModel : ObservableObject
{
    private readonly IProfileService _profileService;
    private readonly ILogger<SecurityViewModel> _logger;

    /// <summary>Initializes a new instance of the <see cref="SecurityViewModel"/> class.</summary>
    public SecurityViewModel(IProfileService profileService, ILogger<SecurityViewModel> logger)
    {
        _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Gets or sets the current security workflow state.</summary>
    [ObservableProperty]
    public partial ProfileState State { get; set; } = ProfileState.Idle;

    /// <summary>Enables or disables two-factor authentication using the default email flow.</summary>
    public async Task<bool> SetTwoFactorEnabled(bool enabled) =>
        enabled ? await EnableTwoFactor(TwoFactorMethod.Email) : await DisableTwoFactor();

    /// <summary>Changes the user's password.</summary>
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

        State = ProfileState.Loading;
        var request = new ChangePasswordRequest(userId, currentPassword, newPassword);
        ErrorOr<Success> result = await _profileService.ChangePasswordAsync(request);
        return result.Match(
            _ =>
            {
                State = ProfileState.UpdateSuccess;
                return (true, string.Empty);
            },
            errors =>
            {
                DesktopLogMessages.ChangePasswordFailed(_logger, errors);
                State = ProfileState.Error;
                string message = errors.First().Code == "incorrect_password"
                    ? UserMessages.Security.IncorrectPassword
                    : UserMessages.Security.UnexpectedError;
                return (false, message);
            });
    }

    /// <summary>Enables two-factor authentication using the specified method.</summary>
    public async Task<bool> EnableTwoFactor(TwoFactorMethod method)
    {
        State = ProfileState.Loading;
        var request = new EnableTwoFaRequest { Method = method };
        ErrorOr<Success> result = await _profileService.Enable2FaAsync(request);
        return result.Match(
            _ =>
            {
                State = ProfileState.UpdateSuccess;
                return true;
            },
            errors =>
            {
                DesktopLogMessages.EnableTwoFactorFailed(_logger, errors);
                State = ProfileState.Error;
                return false;
            });
    }

    /// <summary>Disables two-factor authentication.</summary>
    public async Task<bool> DisableTwoFactor()
    {
        State = ProfileState.Loading;
        ErrorOr<Success> result = await _profileService.Disable2FaAsync();
        return result.Match(
            _ =>
            {
                State = ProfileState.UpdateSuccess;
                return true;
            },
            errors =>
            {
                DesktopLogMessages.DisableTwoFactorFailed(_logger, errors);
                State = ProfileState.Error;
                return false;
            });
    }
}
