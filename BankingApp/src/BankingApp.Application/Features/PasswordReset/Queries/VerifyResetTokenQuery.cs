namespace BankingApp.Application.Features.PasswordReset.Queries;

using System.Security.Cryptography;
using System.Text;
using Common.Utilities;
using Domain.Aggregates.IdentityAggregate;
using Domain.Aggregates.IdentityAggregate.Entities;
using Domain.Common.Errors;
using Domain.Repositories;
using ErrorOr;
using MediatR;

public sealed record VerifyResetTokenQuery(string Token)
    : IRequest<ErrorOr<Success>>;

public sealed class VerifyResetTokenQueryHandler(
    IIdentityRepository identityRepository,
    ISystemClock clock)
    : IRequestHandler<VerifyResetTokenQuery, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(VerifyResetTokenQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.Token))
        {
            return PasswordResetErrors.TokenInvalid;
        }

        string tokenHash = ComputeSha256Hash(query.Token);
        IdentityAccount? identity = await identityRepository.GetByResetTokenHashAsync(tokenHash, cancellationToken);
        if (identity is null)
        {
            return PasswordResetErrors.TokenInvalid;
        }

        PasswordResetToken? token = identity.PasswordResetTokens.FirstOrDefault(t => t.TokenHash == tokenHash);
        if (token is null)
        {
            return PasswordResetErrors.TokenInvalid;
        }

        if (token.UsedAt is not null)
        {
            return PasswordResetErrors.TokenAlreadyUsed;
        }

        if (token.ExpiresAt < clock.UtcNow)
        {
            return PasswordResetErrors.TokenExpired;
        }

        return Result.Success;
    }

    private static string ComputeSha256Hash(string rawData)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
