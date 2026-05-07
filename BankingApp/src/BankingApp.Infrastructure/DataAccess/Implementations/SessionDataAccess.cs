namespace BankingApp.Infrastructure.DataAccess.Implementations;

using Domain.Entities;
using Interfaces;
using ErrorOr;

/// <summary>
///     Provides SQL Server data access for user session records.
/// </summary>
public class SessionDataAccess : ISessionDataAccess
{
    private const int SessionExpirationDays = 7;

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
        try
        {
            Session session = new()
            {
                UserId = userId,
                Token = token,
                DeviceInfo = deviceInfo,
                Browser = browser,
                IpAddress = remoteIpAddress,
                ExpiresAt = DateTime.UtcNow.AddDays(SessionExpirationDays),
                CreatedAt = DateTime.UtcNow,
            };
            _databaseContext.Sessions.Add(session);
            _databaseContext.SaveChanges();
            return session;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }

    /// <inheritdoc />
    /// <param name="token">The token value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Session> FindByToken(string token)
    {
        Session? session = _databaseContext.Sessions.FirstOrDefault(session => session.Token == token && !session.IsRevoked && session.ExpiresAt > DateTime.UtcNow);
        return session ?? (ErrorOr<Session>)Error.NotFound(description: "Session not found.");
    }

    /// <inheritdoc />
    /// <param name="userId">The userId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<List<Session>> FindByUserId(int userId)
    {
        var sessions = _databaseContext.Sessions.Where(session => session.UserId == userId && !session.IsRevoked && session.ExpiresAt > DateTime.UtcNow).ToList();
        return sessions;
    }

    /// <inheritdoc />
    /// <param name="sessionId">The sessionId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> Revoke(int sessionId)
    {
        try
        {
            Session? session = _databaseContext.Sessions.FirstOrDefault(session => session.Id == sessionId && !session.IsRevoked);
            if (session is null)
            {
                return Error.NotFound(description: "Session not found.");
            }

            session.IsRevoked = true;
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
    /// <param name="sessionId">The sessionId value.</param>
    /// <returns>The result of the operation.</returns>
    public ErrorOr<Success> RevokeForUser(int userId, int sessionId)
    {
        try
        {
            Session? session = _databaseContext.Sessions.FirstOrDefault(session => session.Id == sessionId && session.UserId == userId && !session.IsRevoked);
            if (session is null)
            {
                return Error.NotFound(description: "Session not found.");
            }

            session.IsRevoked = true;
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
    public ErrorOr<Success> RevokeAll(int userId)
    {
        try
        {
            Session[] sessions = _databaseContext.Sessions.Where(session => session.UserId == userId && !session.IsRevoked).ToArray();
            foreach (Session session in sessions)
            {
                session.IsRevoked = true;
            }

            _databaseContext.SaveChanges();
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }
}
