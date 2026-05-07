using System;
using System.Linq;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Profile;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Services;
using BankingApp.Desktop.Utilities;
using BankingApp.Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Desktop.ViewModels;

public partial class SecurityViewModel
{
    private readonly IProfileClientService _profileClientService;
    private readonly ILogger<SecurityViewModel> _logger;

    public SecurityViewModel(IProfileClientService profileClientService, ILogger<SecurityViewModel> logger)
    {
        _profileClientService = profileClientService ?? throw new ArgumentNullException(nameof(profileClientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        State = new ObservableState<ProfileState>(ProfileState.Idle);
    }

    public ObservableState<ProfileState> State { get; }

    public async Task<bool> SetTwoFactorEnabled(bool enabled)
    {
        return enabled
            ? await EnableTwoFactor(TwoFactorMethod.Email)
            : await DisableTwoFactor();
    }

    public async Task<(bool Success, string ErrorMessage)> ChangePassword(
        int userId,
        string currentPassword,
        string newPassword,
        string confirmPassword)
    {
        if (!PasswordValidator.MeetsMinimumLength(newPassword))
            return (false, UserMessages.Security.MinimumLengthRequired);

        if (newPassword != confirmPassword) return (false, UserMessages.Security.PasswordMismatch);

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
