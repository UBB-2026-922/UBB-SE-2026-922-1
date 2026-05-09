namespace BankingApp.Infrastructure.DataAccess.Implementations;

using Domain.Entities;
using Interfaces;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

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
        try
        {
            User? user = _databaseContext.Users.Find(userId);
            if (user is null)
            {
                return Error.NotFound(description: "User not found.");
            }

            PasswordResetToken token = new()
            {
                User = user,
                TokenHash = tokenHash,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow,
            };
            _databaseContext.PasswordResetTokens.Add(token);
            _databaseContext.SaveChanges();
            return token;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: $"Failed to create password reset token: {ex.Message}");
        }
    }

    /// <inheritdoc />
    /// <param name="tokenHash">The tokenHash value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<PasswordResetToken> FindByToken(string tokenHash)
    {
        PasswordResetToken? token = _databaseContext.PasswordResetTokens
            .Include(resetToken => resetToken.User)
            .FirstOrDefault(resetToken => resetToken.TokenHash == tokenHash);
        return token ?? (ErrorOr<PasswordResetToken>)Error.NotFound(description: "Password reset token not found.");
    }

    /// <inheritdoc />
    /// <param name="tokenId">The tokenId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> MarkAsUsed(int tokenId)
    {
        try
        {
            PasswordResetToken? token = _databaseContext.PasswordResetTokens.FirstOrDefault(resetToken => resetToken.Id == tokenId);
            if (token is null)
            {
                return Error.NotFound(description: "Password reset token not found.");
            }

            token.UsedAt = DateTime.UtcNow;
            _databaseContext.SaveChanges();
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: $"Failed to mark password reset token as used: {ex.Message}");
        }
    }

    /// <inheritdoc />
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> DeleteExpired()
    {
        try
        {
            var expiredTokens = _databaseContext.PasswordResetTokens.Where(resetToken => resetToken.ExpiresAt < DateTime.UtcNow || resetToken.UsedAt != null).ToList();
            _databaseContext.PasswordResetTokens.RemoveRange(expiredTokens);
            _databaseContext.SaveChanges();
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: $"Failed to delete expired password reset tokens: {ex.Message}");
        }
    }
}
