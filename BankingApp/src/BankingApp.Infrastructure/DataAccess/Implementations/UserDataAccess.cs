// <copyright file="UserDataAccess.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the UserDataAccess class.
// </summary>

using BankingApp.Domain.Entities;
using BankingApp.Infrastructure.DataAccess.Interfaces;
using ErrorOr;

namespace BankingApp.Infrastructure.DataAccess.Implementations;

/// <summary>
///     Provides SQL Server data access for user account records.
/// </summary>
public class UserDataAccess : IUserDataAccess
{
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
        User? user = _databaseContext.Users.FirstOrDefault(user => user.Email == email);
        if (user is null)
        {
            return Error.NotFound(description: "User not found.");
        }

        return user;
    }

    /// <inheritdoc />
    /// <param name="id">The id value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<User> FindById(int id)
    {
        User? user = _databaseContext.Users.FirstOrDefault(user => user.Id == id);
        if (user is null)
        {
            return Error.NotFound(description: "User not found.");
        }

        return user;
    }

    /// <inheritdoc />
    /// <param name="user">The user value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> Create(User user)
    {
        try
        {
            _databaseContext.Users.Add(user);
            _databaseContext.SaveChanges();
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }

    /// <inheritdoc />
    /// <param name="user">The user value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> Update(User user)
    {
        try
        {
            _databaseContext.Users.Update(user);
            _databaseContext.SaveChanges();
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="newPasswordHash">The newPasswordHash value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> UpdatePassword(int userId, string newPasswordHash)
    {
        try
        {
            User? user = _databaseContext.Users.FirstOrDefault(user => user.Id == userId);
            if (user is null)
            {
                return Error.NotFound(description: "User not found.");
            }

            user.PasswordHash = newPasswordHash;
            _databaseContext.SaveChanges();
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> IncrementFailedAttempts(int userId)
    {
        try
        {
            User? user = _databaseContext.Users.FirstOrDefault(user => user.Id == userId);
            if (user is null)
            {
                return Error.NotFound(description: "User not found.");
            }

            user.FailedLoginAttempts++;
            _databaseContext.SaveChanges();
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> ResetFailedAttempts(int userId)
    {
        try
        {
            User? user = _databaseContext.Users.FirstOrDefault(user => user.Id == userId);
            if (user is null)
            {
                return Error.NotFound(description: "User not found.");
            }

            user.FailedLoginAttempts = 0;
            _databaseContext.SaveChanges();
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <param name="lockoutEnd">The lockoutEnd value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> LockAccount(int userId, DateTime lockoutEnd)
    {
        try
        {
            User? user = _databaseContext.Users.FirstOrDefault(user => user.Id == userId);
            if (user is null)
            {
                return Error.NotFound(description: "User not found.");
            }

            user.IsLocked = true;
            user.LockoutEnd = lockoutEnd;
            _databaseContext.SaveChanges();
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }
}
