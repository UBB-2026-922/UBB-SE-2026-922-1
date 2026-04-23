// <copyright file="HashService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the HashService class.
// </summary>

using BankingApp.Application.Services.Security;
using ErrorOr;

namespace BankingApp.Infrastructure.Services.Security;

/// <summary>
///     Provides BCrypt-based password hashing and verification.
/// </summary>
public class HashService : IHashService
{
    /// <inheritdoc />
    /// <param name="input">The input value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<string> GetHash(string input)
    {
        try
        {
            return BCrypt.Net.BCrypt.HashPassword(input);
        }
        catch (Exception exception)
        {
            return Error.Failure("hash.failed", exception.Message);
        }
    }

    /// <inheritdoc />
    /// <param name="input">The input value.</param>
    /// <param name="hash">The hash value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<bool> Verify(string input, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(input, hash);
        }
        catch (Exception exception)
        {
            return Error.Failure("hash.verify_failed", exception.Message);
        }
    }
}