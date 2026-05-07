namespace BankingApp.Infrastructure.Services.Security;

using BankingApp.Application.Services.Security;
using ErrorOr;

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
