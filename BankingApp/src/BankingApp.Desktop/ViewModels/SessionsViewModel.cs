using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DTOs.Profile;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Services;
using BankingApp.Desktop.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Desktop.ViewModels;

public partial class SessionsViewModel
{
    private readonly IProfileClientService _profileClientService;
    private readonly ILogger<SessionsViewModel> _logger;

    public SessionsViewModel(IProfileClientService profileClientService, ILogger<SessionsViewModel> logger)
    {
        _profileClientService = profileClientService ?? throw new ArgumentNullException(nameof(profileClientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        State = new ObservableState<ProfileState>(ProfileState.Idle);
        ActiveSessions = new List<SessionDto>();
    }

    public ObservableState<ProfileState> State { get; }

    public List<SessionDto> ActiveSessions { get; private set; }

    public async Task<bool> LoadSessionsAsync(int userId)
    {
        State.SetValue(ProfileState.Loading);
        try
        {
            ErrorOr<List<SessionDto>> result = await _profileClientService.GetSessionsAsync();
            if (result.IsError)
            {
                ActiveSessions = new List<SessionDto>();
                State.SetValue(ProfileState.Error);
                return false;
            }

            ActiveSessions = result.Value;
            State.SetValue(ProfileState.Idle);
            return true;
        }
        catch (Exception exception)
        {
            _logger.LoadSessionsFailed(exception, userId);
            ActiveSessions = new List<SessionDto>();
            State.SetValue(ProfileState.Error);
            return false;
        }
    }

    public async Task<bool> RevokeSessionAsync(int sessionId)
    {
        State.SetValue(ProfileState.Loading);
        try
        {
            ErrorOr<Success> result = await _profileClientService.RevokeSessionAsync(sessionId);
            State.SetValue(result.IsError ? ProfileState.Error : ProfileState.Idle);
            return !result.IsError;
        }
        catch (Exception exception)
        {
            _logger.RevokeSessionFailed(exception, sessionId);
            State.SetValue(ProfileState.Error);
            return false;
        }
    }
}
