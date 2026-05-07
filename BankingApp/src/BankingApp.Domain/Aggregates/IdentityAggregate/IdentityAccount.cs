namespace BankingApp.Domain.Aggregates.IdentityAggregate;

using Entities;
using Common.Primitives;
using Enums;
using ValueObjects;

public sealed class IdentityAccount : AggregateRoot<int>
{
    private readonly List<Session> _sessions = [];
    private readonly List<PasswordResetToken> _passwordResetTokens = [];

    private IdentityAccount()
    {
    }

    public int UserId { get; private set; }

    public HashedPassword? PasswordHash { get; private set; }

    public bool Is2FaEnabled { get; private set; }

    public TwoFactorMethod? Preferred2FaMethod { get; private set; }

    public bool IsLocked { get; private set; }

    public DateTime? LockoutEnd { get; private set; }

    public int FailedLoginAttempts { get; private set; }

    public IReadOnlyCollection<Session> Sessions => _sessions.AsReadOnly();

    public IReadOnlyCollection<PasswordResetToken> PasswordResetTokens => _passwordResetTokens.AsReadOnly();

    public static IdentityAccount Create(int userId, HashedPassword? passwordHash)
    {
        return new IdentityAccount
        {
            UserId = userId,
            PasswordHash = passwordHash
        };
    }

    public bool IsCurrentlyLocked() => IsLocked && LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;

    public void Enable2Fa(TwoFactorMethod method)
    {
        Is2FaEnabled = true;
        Preferred2FaMethod = method;
    }

    public void Disable2Fa()
    {
        Is2FaEnabled = false;
        Preferred2FaMethod = null;
    }

    public Session OpenSession(string token, DateTime expiresAt, DateTime createdAt, string? deviceInfo = null, string? browser = null, string? ipAddress = null)
    {
        var session = Session.Create(Id, token, expiresAt, createdAt, deviceInfo, browser, ipAddress);
        _sessions.Add(session);
        return session;
    }

    public PasswordResetToken IssuePasswordResetToken(string tokenHash, DateTime expiresAt, DateTime createdAt)
    {
        var resetToken = PasswordResetToken.Create(Id, tokenHash, expiresAt, createdAt);
        _passwordResetTokens.Add(resetToken);
        return resetToken;
    }
}
