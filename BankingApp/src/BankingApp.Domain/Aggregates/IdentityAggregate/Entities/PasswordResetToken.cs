namespace BankingApp.Domain.Aggregates.IdentityAggregate.Entities;

using BankingApp.Domain.Common.Primitives;

public sealed class PasswordResetToken : Entity<int>
{
    private PasswordResetToken()
    {
    }

    public int IdentityAccountId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTime ExpiresAt { get; private set; }

    public DateTime? UsedAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static PasswordResetToken Create(int identityAccountId, string tokenHash, DateTime expiresAt, DateTime createdAt)
    {
        return new PasswordResetToken
        {
            IdentityAccountId = identityAccountId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = createdAt
        };
    }

    public void MarkUsed(DateTime usedAt)
    {
        UsedAt = usedAt;
    }
}
