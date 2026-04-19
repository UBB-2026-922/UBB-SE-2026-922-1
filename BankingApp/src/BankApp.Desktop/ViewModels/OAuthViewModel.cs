// <copyright file="OAuthViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the OAuthViewModel class.
// </summary>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Application.DataTransferObjects.Profile;
using BankApp.Desktop.Enums;
using BankApp.Desktop.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankApp.Desktop.ViewModels;

/// <summary>
///     Handles OAuth provider linking and unlinking operations.
/// </summary>
public class OAuthViewModel
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<OAuthViewModel> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OAuthViewModel" /> class.
    /// </summary>
    /// <param name="apiClient">The API client used for OAuth operations.</param>
    /// <param name="logger">Logger for OAuth operation errors.</param>
    /// <returns>The result of the operation.</returns>
    public OAuthViewModel(IApiClient apiClient, ILogger<OAuthViewModel> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        State = new ObservableState<ProfileState>(ProfileState.Idle);
        OAuthLinks = new List<OAuthLinkDataTransferObject>();
    }

    /// <summary>
    ///     Gets the current OAuth workflow state.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public ObservableState<ProfileState> State { get; }

    /// <summary>
    ///     Gets the linked OAuth accounts for the current user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public List<OAuthLinkDataTransferObject> OAuthLinks { get; private set; }

    /// <summary>
    ///     Loads the OAuth links for the current user from the server.
    /// </summary>
    /// <returns><see langword="true" /> if loaded successfully; otherwise, <see langword="false" />.</returns>
    public async Task<bool> LoadOAuthLinks()
    {
        ErrorOr<List<OAuthLinkDataTransferObject>> oauthResult =
            await _apiClient.GetAsync<List<OAuthLinkDataTransferObject>>(ApiEndpoints.OAuthLinks);
        if (oauthResult.IsError)
        {
            // 404 means no OAuth links exist — treat as success with empty list
            OAuthLinks = new List<OAuthLinkDataTransferObject>();
            return true;
        }

        OAuthLinks = oauthResult.Value;
        return true;
    }

    /// <summary>
    ///     Links a new OAuth provider to the current account.
    /// </summary>
    /// <param name="provider">The provider to link.</param>
    /// <returns><see langword="true" /> if the provider was linked; otherwise, <see langword="false" />.</returns>
    public async Task<bool> LinkOAuth(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return false;
        }

        bool alreadyLinked = OAuthLinks.Exists(link =>
            string.Equals(link.Provider, provider, StringComparison.OrdinalIgnoreCase));
        if (alreadyLinked)
        {
            return false;
        }

        State.SetValue(ProfileState.Loading);
        string trimmedProvider = provider.Trim();
        var request = new { Provider = trimmedProvider };
        ErrorOr<Success> result = await _apiClient.PostAsync(ApiEndpoints.LinkOAuth, request);
        return result.Match(
            _ =>
            {
                OAuthLinks.Add(new OAuthLinkDataTransferObject { Provider = trimmedProvider });
                State.SetValue(ProfileState.UpdateSuccess);
                return true;
            },
            errors =>
            {
                _logger.LogError("LinkOAuth failed: {Errors}", errors);
                State.SetValue(ProfileState.Error);
                return false;
            });
    }

    /// <summary>
    ///     Removes a linked OAuth provider from the local profile state.
    /// </summary>
    /// <param name="provider">The provider to remove.</param>
    /// <returns><see langword="true" /> if the provider was removed; otherwise, <see langword="false" />.</returns>
    public async Task<bool> UnlinkOAuth(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return false;
        }

        OAuthLinkDataTransferObject? existing = OAuthLinks.Find(link =>
            string.Equals(link.Provider, provider, StringComparison.OrdinalIgnoreCase));
        if (existing == null)
        {
            return false;
        }

        State.SetValue(ProfileState.Loading);
        ErrorOr<Success> result =
            await _apiClient.DeleteAsync($"{ApiEndpoints.UnlinkOAuth}/{Uri.EscapeDataString(provider.Trim())}");
        if (result.IsError)
        {
            _logger.LogError("UnlinkOAuth failed: {Errors}", result.Errors);
            State.SetValue(ProfileState.Error);
            return false;
        }

        OAuthLinks.Remove(existing);
        State.SetValue(ProfileState.UpdateSuccess);
        return true;
    }
}