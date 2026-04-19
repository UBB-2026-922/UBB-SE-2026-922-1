// <copyright file="LoginSuccess.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains code for LoginSuccess.
// </summary>

namespace BankApp.Application.DataTransferObjects.Auth;

/// <summary>
///     Represents a successful login outcome. Pattern-match on the concrete type to
///     distinguish a completed login from a pending 2FA step.
/// </summary>
public abstract class LoginSuccess
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LoginSuccess" /> class.
    /// </summary>
    /// <param name="userId">The identifier of the authenticated user.</param>
    /// <returns>The result of the operation.</returns>
    protected LoginSuccess(int userId)
    {
        UserId = userId;
    }

    /// <summary>
    ///     Gets the identifier of the authenticated user.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public int UserId { get; }
}

/// <summary>
///     Represents a completed login where a JWT has been issued and the session is active.
/// </summary>
public sealed class FullLogin : LoginSuccess
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FullLogin" /> class.
    /// </summary>
    /// <param name="userId">The identifier of the authenticated user.</param>
    /// <param name="token">The signed JWT for subsequent authenticated requests.</param>
    /// <returns>The result of the operation.</returns>
    public FullLogin(int userId, string token)
        : base(userId)
    {
        Token = token;
    }

    /// <summary>
    ///     Gets the signed JWT for subsequent authenticated requests.
    /// </summary>
    /// <value>
    ///     Gets or sets the current value.
    /// </value>
    public string Token { get; }
}

/// <summary>
///     Represents a partial login where 2FA must be completed
///     before a token is issued.
/// </summary>
public sealed class RequiresTwoFactor : LoginSuccess
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RequiresTwoFactor" /> class.
    /// </summary>
    /// <param name="userId">The identifier of the partially-authenticated user.</param>
    /// <returns>The result of the operation.</returns>
    public RequiresTwoFactor(int userId)
        : base(userId)
    {
    }
}