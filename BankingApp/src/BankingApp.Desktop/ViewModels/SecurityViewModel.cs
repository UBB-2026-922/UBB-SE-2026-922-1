namespace BankingApp.Desktop.ViewModels;

using System;
using System.Linq;
using System.Threading.Tasks;
using BankingApp.Application.Features.UserProfile.Dtos;
using Enums;
using BankingApp.Desktop.Services;
using BankingApp.Application.Common.Utilities;
using BankingApp.Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging;

/// <summary>Handles password changes and two-factor authentication settings for the profile area.</summary>
public partial class SecurityViewModel : ObservableObject
{
    private readonly IProfileClientService _profileClientService;
    private readonly ILogger<SecurityViewModel> _logger;

    /// <summary>Initializes a new instance of the <see cref="SecurityViewModel"/> class.</summary>
    public SecurityViewModel(IProfileClientService profileClientService, ILogger<SecurityViewModel> logger)
    {
        _profileClientService = profileClientService ?? throw new ArgumentNullException(nameof(profileClientService));
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
        ErrorOr<Success> result = await _profileClientService.ChangePasswordAsync(request);
        return result.Match(
            _ =>
            {
                State = ProfileState.UpdateSuccess;
                return (true, string.Empty);
            },
            errors =>
            {
                _logger.ChangePasswordFailed(errors);
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
        ErrorOr<Success> result = await _profileClientService.Enable2FaAsync(request);
        return result.Match(
            _ =>
            {
                State = ProfileState.UpdateSuccess;
                return true;
            },
            errors =>
            {
                _logger.EnableTwoFactorFailed(errors);
                State = ProfileState.Error;
                return false;
            });
    }

    /// <summary>Disables two-factor authentication.</summary>
    public async Task<bool> DisableTwoFactor()
    {
        State = ProfileState.Loading;
        ErrorOr<Success> result = await _profileClientService.Disable2FaAsync();
        return result.Match(
            _ =>
            {
                State = ProfileState.UpdateSuccess;
                return true;
            },
            errors =>
            {
                _logger.DisableTwoFactorFailed(errors);
                State = ProfileState.Error;
                return false;
            });
    }
}
