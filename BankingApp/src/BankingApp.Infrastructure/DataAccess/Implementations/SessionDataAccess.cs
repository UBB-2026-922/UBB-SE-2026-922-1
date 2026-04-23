// <copyright file="SessionDataAccess.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the SessionDataAccess class.
// </summary>

using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankingApp.Infrastructure.DataAccess.Implementations;

/// <summary>
///     Provides SQL Server data access for user session records.
/// </summary>
public class SessionDataAccess : ISessionDataAccess
{
    private const int SessionExpirationDays = 7;

    private const string SelectAllColumns = """
                                            SELECT Id, UserId, Token, DeviceInfo, Browser, IpAddress,
                                                   LastActiveAt, ExpiresAt, IsRevoked, CreatedAt
                                            FROM [Session]
                                            """;

    private readonly AppDatabaseContext _databaseContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SessionDataAccess" /> class.
    /// </summary>
    /// <param name="databaseContext">The database context used for executing queries.</param>
    /// <returns>The result of the operation.</returns>
    public SessionDataAccess(AppDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="token">The token value.</param>
    /// <param name="deviceInfo">The deviceInfo value.</param>
    /// <param name="browser">The browser value.</param>
    /// <param name="remoteIpAddress">The remote IP address value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Session> Create(
        int userId,
        string token,
        string? deviceInfo,
        string? browser,
        string? remoteIpAddress)
    {
        const string databaseCommandText = """
                                           INSERT INTO [Session] (UserId, Token, DeviceInfo, Browser, IpAddress, LastActiveAt, ExpiresAt)
                                           OUTPUT INSERTED.Id, INSERTED.UserId, INSERTED.Token, INSERTED.DeviceInfo,
                                                  INSERTED.Browser, INSERTED.IpAddress, INSERTED.LastActiveAt,
                                                  INSERTED.ExpiresAt, INSERTED.IsRevoked, INSERTED.CreatedAt
                                           VALUES (@UserId, @Token, @DeviceInfo, @Browser, @IpAddress,
                                                   GETUTCDATE(), DATEADD(DAY, @ExpirationDays, GETUTCDATE()))
                                           """;
        return _databaseContext.Query(connection => connection.QueryFirstOrDefault<Session>(
            databaseCommandText,
            new
            {
                UserId = userId,
                Token = token,
                DeviceInfo = deviceInfo,
                Browser = browser,
                IpAddress = remoteIpAddress,
                ExpirationDays = SessionExpirationDays,
            })).Then(session => session ?? (ErrorOr<Session>)Error.Failure(description: "Failed to create session."));
    }

    /// <inheritdoc />
    /// <param name="token">The token value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Session> FindByToken(string token)
    {
        const string query = $"{SelectAllColumns} WHERE Token = @Token AND IsRevoked = 0 AND ExpiresAt > GETUTCDATE()";
        return _databaseContext
            .Query(connection => connection.QueryFirstOrDefault<Session>(query, new { Token = token }))
            .Then(session => session ?? (ErrorOr<Session>)Error.NotFound(description: "Session not found."));
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<Session>> FindByUserId(int userId)
    {
        const string query =
            $"{SelectAllColumns} WHERE UserId = @UserId AND IsRevoked = 0 AND ExpiresAt > GETUTCDATE()";
        return _databaseContext.Query(connection => connection.Query<Session>(query, new { UserId = userId }).AsList());
    }

    /// <inheritdoc />
    /// <param name="sessionId">The sessionId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> Revoke(int sessionId)
    {
        const string databaseCommandText = "UPDATE [Session] SET IsRevoked = 1 WHERE Id = @Id";
        return _databaseContext.Query(connection => connection.Execute(databaseCommandText, new { Id = sessionId }))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="sessionId">The sessionId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> RevokeForUser(int userId, int sessionId)
    {
        const string databaseCommandText = """
                                           UPDATE [Session]
                                           SET IsRevoked = 1
                                           WHERE Id = @SessionId
                                             AND UserId = @UserId
                                             AND IsRevoked = 0
                                             AND ExpiresAt > GETUTCDATE()
                                           """;
        return _databaseContext.Query(connection => connection.Execute(
                databaseCommandText,
                new { UserId = userId, SessionId = sessionId }))
            .Then(rowsAffected => rowsAffected > default(int)
                ? Result.Success
                : (ErrorOr<Success>)Error.NotFound(description: "Session not found."));
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> RevokeAll(int userId)
    {
        const string databaseCommandText =
            "UPDATE [Session] SET IsRevoked = 1 WHERE UserId = @UserId AND IsRevoked = 0";
        return _databaseContext.Query(connection => connection.Execute(databaseCommandText, new { UserId = userId }))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }
}