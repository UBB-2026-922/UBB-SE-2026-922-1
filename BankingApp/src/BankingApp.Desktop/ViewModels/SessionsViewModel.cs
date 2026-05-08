namespace BankingApp.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.Features.UserProfile.Dtos;
using Enums;
using BankingApp.Desktop.Services;
using BankingApp.Application.Common.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

/// <summary>Handles active-session loading and revocation for the profile area.</summary>
public partial class SessionsViewModel : ObservableObject
{
    private readonly IProfileClientService _profileClientService;
    private readonly ILogger<SessionsViewModel> _logger;

    /// <summary>Initializes a new instance of the <see cref="SessionsViewModel"/> class.</summary>
    public SessionsViewModel(IProfileClientService profileClientService, ILogger<SessionsViewModel> logger)
    {
        _profileClientService = profileClientService ?? throw new ArgumentNullException(nameof(profileClientService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ActiveSessions = new List<SessionDto>();
    }

    /// <summary>Gets or sets the current sessions workflow state.</summary>
    [ObservableProperty]
    public partial ProfileState State { get; set; } = ProfileState.Idle;

    /// <summary>Gets the active sessions currently loaded for the user.</summary>
    public List<SessionDto> ActiveSessions { get; private set; }

    /// <summary>Loads active sessions for the current authenticated user.</summary>
    public async Task<bool> LoadSessionsAsync(int userId)
    {
        State = ProfileState.Loading;
        try
        {
            ErrorOr<List<SessionDto>> result = await _profileClientService.GetSessionsAsync();
            if (result.IsError)
            {
                ActiveSessions = new List<SessionDto>();
                State = ProfileState.Error;
                return false;
            }

            ActiveSessions = result.Value;
            State = ProfileState.Idle;
            return true;
        }
        catch (Exception exception)
        {
            _logger.LoadSessionsFailed(exception, userId);
            ActiveSessions = new List<SessionDto>();
            State = ProfileState.Error;
            return false;
        }
    }

    /// <summary>Revokes the specified active session.</summary>
    public async Task<bool> RevokeSessionAsync(int sessionId)
    {
        State = ProfileState.Loading;
        try
        {
            ErrorOr<Success> result = await _profileClientService.RevokeSessionAsync(sessionId);
            State = result.IsError ? ProfileState.Error : ProfileState.Idle;
            return !result.IsError;
        }
        catch (Exception exception)
        {
            _logger.RevokeSessionFailed(exception, sessionId);
            State = ProfileState.Error;
            return false;
        }
    }
}
