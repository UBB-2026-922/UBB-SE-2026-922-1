// <copyright file="OAuthLinkDataAccess.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the OAuthLinkDataAccess class.
// </summary>

using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.DataAccess.Interfaces;
using ErrorOr;

namespace BankingApp.Infrastructure.DataAccess.Implementations;

/// <summary>
///     Provides SQL Server data access for OAuth provider link records.
/// </summary>
public class OAuthLinkDataAccess : IOAuthLinkDataAccess
{

    private readonly AppDatabaseContext _databaseContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OAuthLinkDataAccess" /> class.
    /// </summary>
    /// <param name="databaseContext">The database context used for executing queries.</param>
    /// <returns>The result of the operation.</returns>
    public OAuthLinkDataAccess(AppDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="provider">The provider value.</param>
    /// <param name="providerUserId">The providerUserId value.</param>
    /// <param name="providerEmail">The providerEmail value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> Create(int userId, string provider, string providerUserId, string? providerEmail)
    {
        try
        {
            OAuthLink link = new()
            {
                UserId = userId,
                Provider = provider,
                ProviderUserId = providerUserId,
                ProviderEmail = providerEmail,
            };
            _databaseContext.OAuthLinks.Add(link);
            _databaseContext.SaveChanges();
            return Result.Success;

        }
        catch (Exception ex)
        {
            return Error.Failure(description: $"Failed to create OAuth link: {ex.Message}");
        }
    }

    /// <inheritdoc />
    /// <param name="id">The id value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> Delete(int id)
    {
        try
        {
            OAuthLink? link = _databaseContext.OAuthLinks.FirstOrDefault(oauthLink => oauthLink.Id == id);
            if (link is null)
            {
                return Error.NotFound(description: "OAuth link not found.");
            }

            _databaseContext.OAuthLinks.Remove(link);
            _databaseContext.SaveChanges();
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }

    /// <inheritdoc />
    /// <param name="provider">The provider value.</param>
    /// <param name="providerUserId">The providerUserId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<OAuthLink> FindByProvider(string provider, string providerUserId)
    {
        OAuthLink? link = _databaseContext.OAuthLinks.FirstOrDefault(oauthLink =>
            oauthLink.Provider == provider && oauthLink.ProviderUserId == providerUserId);
        if (link == null)
        {
            return Error.NotFound(description: "OAuth link not found.");
        }

        return link;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<OAuthLink>> FindByUserId(int userId)
    {
        List<OAuthLink> links = _databaseContext.OAuthLinks.Where(oauthLink => oauthLink.UserId == userId).ToList();
        return links;
    }
}