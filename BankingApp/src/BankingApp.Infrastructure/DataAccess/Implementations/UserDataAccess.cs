// <copyright file="UserDataAccess.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the UserDataAccess class.
// </summary>

using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.DataAccess.Interfaces;
using Dapper;
using ErrorOr;

namespace BankingApp.Infrastructure.DataAccess.Implementations;

/// <summary>
///     Provides SQL Server data access for user account records.
/// </summary>
public class UserDataAccess : IUserDataAccess
{
    private const string SelectAllColumns = """
                                            SELECT Id, Email, PasswordHash, FullName, PhoneNumber, DateOfBirth,
                                                   [Address], Nationality, PreferredLanguage,
                                                   Is2FAEnabled AS Is2FAEnabled, Preferred2FaMethod,
                                                   IsLocked, LockoutEnd, FailedLoginAttempts, CreatedAt, UpdatedAt
                                            FROM [User]
                                            """;

    private readonly AppDatabaseContext _databaseContext;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserDataAccess" /> class.
    /// </summary>
    /// <param name="databaseContext">The database context used for executing queries.</param>
    /// <returns>The result of the operation.</returns>
    public UserDataAccess(AppDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    /// <inheritdoc />
    /// <param name="email">The email value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<User> FindByEmail(string email)
    {
        const string query = $"{SelectAllColumns} WHERE Email = @Email";
        return _databaseContext.Query(connection => connection.QueryFirstOrDefault<User>(query, new { Email = email }))
            .Then(user => user ?? (ErrorOr<User>)Error.NotFound(description: "User not found."));
    }

    /// <inheritdoc />
    /// <param name="id">The id value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<User> FindById(int id)
    {
        const string query = $"{SelectAllColumns} WHERE Id = @Id";
        return _databaseContext.Query(connection => connection.QueryFirstOrDefault<User>(query, new { Id = id }))
            .Then(user => user ?? (ErrorOr<User>)Error.NotFound(description: "User not found."));
    }

    /// <inheritdoc />
    /// <param name="user">The user value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> Create(User user)
    {
        const string databaseCommandText = """
                                           INSERT INTO [User] (Email, PasswordHash, FullName, PhoneNumber, DateOfBirth,
                                               [Address], Nationality, PreferredLanguage, Is2FAEnabled, Preferred2FaMethod)
                                           VALUES (@Email, @PasswordHash, @FullName, @PhoneNumber, @DateOfBirth,
                                               @Address, @Nationality, @PreferredLanguage, @Is2FAEnabled, @Preferred2FaMethod)
                                           """;
        return _databaseContext.Query(connection => connection.Execute(databaseCommandText, user))
            .Then(rows =>
                rows > default(int)
                    ? Result.Success
                    : (ErrorOr<Success>)Error.Failure(description: "Failed to create user."));
    }

    /// <inheritdoc />
    /// <param name="user">The user value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> Update(User user)
    {
        const string databaseCommandText = """
                                           UPDATE [User]
                                           SET Email              = @Email,
                                               FullName           = @FullName,
                                               PhoneNumber        = @PhoneNumber,
                                               DateOfBirth        = @DateOfBirth,
                                               [Address]          = @Address,
                                               Nationality        = @Nationality,
                                               PreferredLanguage  = @PreferredLanguage,
                                               Is2FAEnabled       = @Is2FAEnabled,
                                               Preferred2FaMethod = @Preferred2FaMethod,
                                               UpdatedAt          = GETUTCDATE()
                                           WHERE Id = @Id
                                           """;
        return _databaseContext.Query(connection => connection.Execute(databaseCommandText, user))
            .Then(rows =>
                rows > default(int)
                    ? Result.Success
                    : (ErrorOr<Success>)Error.Failure(description: "Failed to update user."));
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="newPasswordHash">The newPasswordHash value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> UpdatePassword(int userId, string newPasswordHash)
    {
        const string databaseCommandText = """
                                           UPDATE [User]
                                           SET PasswordHash = @PasswordHash,
                                               UpdatedAt    = GETUTCDATE()
                                           WHERE Id = @UserId
                                           """;
        return _databaseContext.Query(connection => connection.Execute(
                databaseCommandText,
                new { UserId = userId, PasswordHash = newPasswordHash }))
            .Then(rows =>
                rows > default(int)
                    ? Result.Success
                    : (ErrorOr<Success>)Error.Failure(description: "Failed to update password."));
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> IncrementFailedAttempts(int userId)
    {
        const string databaseCommandText =
            "UPDATE [User] SET FailedLoginAttempts = FailedLoginAttempts + 1 WHERE Id = @UserId";
        return _databaseContext.Query(connection => connection.Execute(databaseCommandText, new { UserId = userId }))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> ResetFailedAttempts(int userId)
    {
        const string databaseCommandText = "UPDATE [User] SET FailedLoginAttempts = default WHERE Id = @UserId";
        return _databaseContext.Query(connection => connection.Execute(databaseCommandText, new { UserId = userId }))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="lockoutEnd">The lockoutEnd value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> LockAccount(int userId, DateTime lockoutEnd)
    {
        const string databaseCommandText =
            "UPDATE [User] SET IsLocked = 1, LockoutEnd = @LockoutEnd WHERE Id = @UserId";
        return _databaseContext.Query(connection => connection.Execute(
                databaseCommandText,
                new { UserId = userId, LockoutEnd = lockoutEnd }))
            .Then(_ => (ErrorOr<Success>)Result.Success);
    }
}