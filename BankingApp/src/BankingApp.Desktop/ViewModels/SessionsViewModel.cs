// <copyright file="SessionsViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the SessionsViewModel class.
// </summary>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankingApp.Application.DataTransferObjects.Profile;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankingApp.Desktop.ViewModels;

/// <summary>
///     Handles active session management for the current user.
/// </summary>
public class SessionsViewModel
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<SessionsViewModel> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SessionsViewModel" /> class.
    /// </summary>
    /// <param name="apiClient">The API client used for session operations.</param>
    /// <param name="logger">Logger for session operation errors.</param>
    /// <returns>The result of the operation.</returns>
    public SessionsViewModel(IApiClient apiClient, ILogger<SessionsViewModel> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        State = new ObservableState<ProfileState>(ProfileState.Idle);
        ActiveSessions = new List<SessionDataTransferObject>();
    }

    /// <summary>
    ///     Gets the current sessions workflow state.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ObservableState<ProfileState> State { get; }

    /// <summary>
    ///     Gets the active sessions for the current user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public List<SessionDataTransferObject> ActiveSessions { get; private set; }

    /// <summary>
    ///     Loads all active sessions for the specified user from the server.
    /// </summary>
    /// <param name="userId">The identifier of the current user.</param>
    /// <returns><see langword="true" /> if sessions loaded successfully; otherwise, <see langword="false" />.</returns>
    public async Task<bool> LoadSessionsAsync(int userId)
    {
        State.SetValue(ProfileState.Loading);
        try
        {
            ErrorOr<List<SessionDataTransferObject>> result =
                await _apiClient.GetAsync<List<SessionDataTransferObject>>(ApiEndpoints.Sessions);
            if (result.IsError)
            {
                ActiveSessions = new List<SessionDataTransferObject>();
                State.SetValue(ProfileState.Error);
                return false;
            }

            ActiveSessions = result.Value;
            State.SetValue(ProfileState.Idle);
            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to load sessions for user {UserId}", userId);
            ActiveSessions = new List<SessionDataTransferObject>();
            State.SetValue(ProfileState.Error);
            return false;
        }
    }

    /// <summary>
    ///     Revokes a specific session by its identifier.
    /// </summary>
    /// <param name="sessionId">The identifier of the session to revoke.</param>
    /// <returns><see langword="true" /> if the session was revoked successfully; otherwise <see langword="false" />.</returns>
    public async Task<bool> RevokeSessionAsync(int sessionId)
    {
        State.SetValue(ProfileState.Loading);
        try
        {
            ErrorOr<Success> result = await _apiClient.DeleteAsync($"{ApiEndpoints.Sessions}/{sessionId}");
            State.SetValue(result.IsError ? ProfileState.Error : ProfileState.Idle);
            return !result.IsError;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to revoke session {SessionId}", sessionId);
            State.SetValue(ProfileState.Error);
            return false;
        }
    }
}