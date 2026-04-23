// <copyright file="SessionValidationMiddlewareTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Api.Middleware;
using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Security;
using BankApp.Domain.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BankApp.Api.Tests.Middleware;

/// <summary>
///     Unit tests for <see cref="SessionValidationMiddleware" /> verifying
///     public-versus-protected endpoint behavior and auth token validation.
/// </summary>
[Trait("Category", "Unit")]
public sealed class SessionValidationMiddlewareTests
{
    private readonly Mock<IAuthRepository> _authenticationRepository = MockFactory.CreateAuthRepository();
    private readonly Mock<IJsonWebTokenService> _jwtService = MockFactory.CreateJwtService();
    private readonly Mock<ILogger<SessionValidationMiddleware>> _logger = new();

    private bool _nextWasCalled;

    [Theory]
    [InlineData("/api/auth/login")]
    [InlineData("/api/auth/register")]
    [InlineData("/swagger/index.html")]
    public async Task Invoke_PublicEndpoint_CallsNextWithoutToken(string path)
    {
        // Arrange
        SessionValidationMiddleware middleware = CreateMiddleware();
        HttpContext context = CreateHttpContext(path);

        // Act
        await middleware.Invoke(context, _authenticationRepository.Object, _jwtService.Object, _logger.Object);

        // Assert
        _nextWasCalled.Should().BeTrue();
        context.Response.StatusCode.Should().NotBe(401);
    }

    [Fact]
    public async Task Invoke_ProtectedEndpointHasNoToken_Returns401()
    {
        // Arrange
        SessionValidationMiddleware middleware = CreateMiddleware();
        HttpContext context = CreateHttpContext("/api/dashboard");

        // Act
        await middleware.Invoke(context, _authenticationRepository.Object, _jwtService.Object, _logger.Object);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        _nextWasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Invoke_ProtectedEndpointHasInvalidToken_Returns401()
    {
        // Arrange
        _jwtService
            .Setup(extractsUserId => extractsUserId.ExtractUserId("bad-token"))
            .Returns(Error.Validation("token_invalid", "Invalid token."));

        SessionValidationMiddleware middleware = CreateMiddleware();
        HttpContext context = CreateHttpContext("/api/profile", "Bearer bad-token");

        // Act
        await middleware.Invoke(context, _authenticationRepository.Object, _jwtService.Object, _logger.Object);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        _nextWasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Invoke_ProtectedEndpointHasValidTokenButNoSession_Returns401()
    {
        // Arrange
        const int validUserId = 1;
        _jwtService.Setup(extractsUserId => extractsUserId.ExtractUserId("good-token")).Returns(validUserId);
        _authenticationRepository
            .Setup(checksSession => checksSession.IsSessionActive("good-token"))
            .Returns(Error.NotFound("session_not_found", "Session not found."));

        SessionValidationMiddleware middleware = CreateMiddleware();
        HttpContext context = CreateHttpContext("/api/dashboard", "Bearer good-token");

        // Act
        await middleware.Invoke(context, _authenticationRepository.Object, _jwtService.Object, _logger.Object);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        _nextWasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Invoke_ProtectedEndpointHasValidTokenAndSession_CallsNextAndSetsUserId()
    {
        // Arrange
        const int validUserId = 42;
        _jwtService.Setup(extractsUserId => extractsUserId.ExtractUserId("good-token")).Returns(validUserId);
        _authenticationRepository
            .Setup(checksSession => checksSession.IsSessionActive("good-token"))
            .Returns(true);

        SessionValidationMiddleware middleware = CreateMiddleware();
        HttpContext context = CreateHttpContext("/api/profile", "Bearer good-token");

        // Act
        await middleware.Invoke(context, _authenticationRepository.Object, _jwtService.Object, _logger.Object);

        // Assert
        _nextWasCalled.Should().BeTrue();
        context.Items["UserId"].Should().Be(validUserId);
    }

    [Fact]
    public async Task Invoke_ProtectedEndpoint_MalformedAuthHeader_Returns401()
    {
        // Arrange
        SessionValidationMiddleware middleware = CreateMiddleware();
        HttpContext context = CreateHttpContext("/api/dashboard", "Basic some-creds");

        // Act
        await middleware.Invoke(context, _authenticationRepository.Object, _jwtService.Object, _logger.Object);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        _nextWasCalled.Should().BeFalse();
    }

    [Theory]
    [InlineData("/api/profile/oauth-auth/revoke")]
    public async Task Invoke_ProtectedEndpoint_PathContainingAuthPrefixButNotStartingWithIt_Returns401(string path)
    {
        // Arrange
        SessionValidationMiddleware middleware = CreateMiddleware();
        HttpContext context = CreateHttpContext(path);

        // Act
        await middleware.Invoke(context, _authenticationRepository.Object, _jwtService.Object, _logger.Object);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        _nextWasCalled.Should().BeFalse();
    }

    private static HttpContext CreateHttpContext(string path, string? authorizationHeader = null)
    {
        var context = new DefaultHttpContext
        {
            Request =
            {
                Path = path,
            },
        };
        if (authorizationHeader != null)
        {
            context.Request.Headers.Authorization = authorizationHeader;
        }

        return context;
    }

    private SessionValidationMiddleware CreateMiddleware()
    {
        _nextWasCalled = false;
        return new SessionValidationMiddleware(_ =>
        {
            _nextWasCalled = true;
            return Task.CompletedTask;
        });
    }
}
