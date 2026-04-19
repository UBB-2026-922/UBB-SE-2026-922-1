// <copyright file="PasswordResetTokenDataAccess.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the PasswordResetTokenDataAccess class.
// </summary>

using BankApp.Domain.Entities;
using BankApp.Infrastructure.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankApp.Infrastructure.DataAccess.Implementations;

/// <summary>
///     Provides SQL Server data access for password reset token records.
/// </summary>
public class PasswordResetTokenDataAccess : IPasswordResetTokenDataAccess
{
    private readonly AppDatabaseContext _databaseContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PasswordResetTokenDataAccess" /> class.
    /// </summary>
    /// <param name="databaseContext">The database context used for executing queries.</param>
    /// <returns>The result of the operation.</returns>
    public PasswordResetTokenDataAccess(AppDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="tokenHash">The tokenHash value.</param>
    /// <param name="expiresAt">The expiresAt value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<PasswordResetToken> Create(int userId, string tokenHash, DateTime expiresAt)
    {
        const string databaseCommandText = """
                                           INSERT INTO PasswordResetToken (UserId, TokenHash, ExpiresAt)
                                           OUTPUT INSERTED.Id, INSERTED.UserId, INSERTED.TokenHash,
                                                  INSERTED.ExpiresAt, INSERTED.UsedAt, INSERTED.CreatedAt
                                           VALUES (@UserId, @TokenHash, @ExpiresAt)
                                           """;
        return _databaseContext.Query(connection => connection.QueryFirstOrDefault<PasswordResetToken>(
            databaseCommandText,
            new
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = expiresAt,
            })).Then(token =>
            token ?? (ErrorOr<PasswordResetToken>)Error.Failure(description: "Failed to create password reset token."));
    }

    /// <inheritdoc />
    /// <param name="tokenHash">The tokenHash value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<PasswordResetToken> FindByToken(string tokenHash)
    {
        const string databaseCommandText = """
                                           SELECT Id, UserId, TokenHash, ExpiresAt, UsedAt, CreatedAt
                                           FROM PasswordResetToken
                                           WHERE TokenHash = @TokenHash
                                           """;
        return _databaseContext.Query(connection =>
                connection.QueryFirstOrDefault<PasswordResetToken>(databaseCommandText, new { TokenHash = tokenHash }))
            .Then(token =>
                token ?? (ErrorOr<PasswordResetToken>)Error.NotFound(description: "Password reset token not found."));
    }

    /// <inheritdoc />
    /// <param name="tokenId">The tokenId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> MarkAsUsed(int tokenId)
    {
        const string databaseCommandText = "UPDATE PasswordResetToken SET UsedAt = GETUTCDATE() WHERE Id = @Id";
        return _databaseContext.Query(connection => connection.Execute(databaseCommandText, new { Id = tokenId }))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> DeleteExpired()
    {
        const string databaseCommandText = "DELETE FROM PasswordResetToken WHERE ExpiresAt < GETUTCDATE()";
        return _databaseContext.Query(connection => connection.Execute(databaseCommandText))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }
}