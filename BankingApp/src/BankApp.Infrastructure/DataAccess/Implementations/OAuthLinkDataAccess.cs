// <copyright file="OAuthLinkDataAccess.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the OAuthLinkDataAccess class.
// </summary>

using BankApp.Domain.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Infrastructure.DataAccess.Implementations;

/// <summary>
///     Provides SQL Server data access for OAuth provider link records.
/// </summary>
public class OAuthLinkDataAccess : IOAuthLinkDataAccess
{
    private const string SelectAllColumns = """
                                            SELECT Id, UserId, Provider, ProviderUserId, ProviderEmail, LinkedAt
                                            FROM OAuthLink
                                            """;

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
        const string databaseCommandText = """
                                           INSERT INTO OAuthLink (UserId, Provider, ProviderUserId, ProviderEmail)
                                           VALUES (@UserId, @Provider, @ProviderUserId, @ProviderEmail)
                                           """;
        return _databaseContext.Query(connection => connection.Execute(
            databaseCommandText,
            new
            {
                UserId = userId,
                Provider = provider,
                ProviderUserId = providerUserId,
                ProviderEmail = providerEmail,
            })).Then(rows =>
            rows > 0
                ? Result.Success
                : (ErrorOr<Success>)Error.Failure(description: "Failed to create OAuth link."));
    }

    /// <inheritdoc />
    /// <param name="id">The id value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> Delete(int id)
    {
        const string databaseCommandText = "DELETE FROM OAuthLink WHERE Id = @Id";
        return _databaseContext.Query(connection => connection.Execute(databaseCommandText, new { Id = id }))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }

    /// <inheritdoc />
    /// <param name="provider">The provider value.</param>
    /// <param name="providerUserId">The providerUserId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<OAuthLink> FindByProvider(string provider, string providerUserId)
    {
        const string query = $"{SelectAllColumns} WHERE Provider = @Provider AND ProviderUserId = @ProviderUserId";
        return _databaseContext.Query(connection => connection.QueryFirstOrDefault<OAuthLink>(
                query,
                new { Provider = provider, ProviderUserId = providerUserId }))
            .Then(link => link ?? (ErrorOr<OAuthLink>)Error.NotFound(description: "OAuth link not found."));
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<OAuthLink>> FindByUserId(int userId)
    {
        const string query = $"{SelectAllColumns} WHERE UserId = @UserId";
        return _databaseContext.Query(connection =>
            connection.Query<OAuthLink>(query, new { UserId = userId }).AsList());
    }
}