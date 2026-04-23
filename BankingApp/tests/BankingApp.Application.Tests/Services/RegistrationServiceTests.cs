// <copyright file="RegistrationServiceTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankingApp.Application.DataTransferObjects.Auth;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.Registration;
using BankingApp.Application.Services.Security;
using BankingApp.Domain.Entities;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankingApp.Application.Tests.Services;

/// <summary>
///     Unit tests for <see cref="RegistrationService" />.
/// </summary>
public class RegistrationServiceTests
{
    private readonly Mock<IAuthRepository> _authRepository = MockFactory.CreateAuthRepository();
    private readonly Mock<IHashService> _hashService = MockFactory.CreateHashService();
    private readonly RegistrationService _service;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RegistrationServiceTests" /> class.
    /// </summary>
    public RegistrationServiceTests()
    {
        _service = new RegistrationService(
            _authRepository.Object,
            _hashService.Object,
            NullLogger<RegistrationService>.Instance);
    }

    [Fact]
    public void Register_WhenExistingUserLookupFails_ReturnsDatabaseError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "new@test.com",
            Password = "StrongPass1!",
            FullName = "New User",
        };
        _authRepository
            .Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns(Error.Failure("db_failed", "Database failed."));

        // Act
        ErrorOr<Success> result = _service.Register(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("database_error");
    }

    [Fact]
    public void Register_WhenValid_CreatesUserWithDefaults()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "new@test.com",
            Password = "StrongPass1!",
            FullName = "New User",
        };
        _authRepository
            .Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns(Error.NotFound());
        _hashService
            .Setup(getsHash => getsHash.GetHash(request.Password))
            .Returns("hashed-password");
        _authRepository
            .Setup(createsUser => createsUser.CreateUser(It.IsAny<User>()))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _service.Register(request);

        // Assert
        result.IsError.Should().BeFalse();
        _authRepository.Verify(
            createsUser => createsUser.CreateUser(
                It.Is<User>(user =>
                    user.Email == request.Email &&
                    user.FullName == request.FullName &&
                    user.PasswordHash == "hashed-password" &&
                    user.PreferredLanguage == "en" &&
                    !user.Is2FaEnabled &&
                    !user.IsLocked &&
                    user.FailedLoginAttempts == 0)),
            Times.Once);
    }

    [Fact]
    public void OAuthRegister_WhenLinkLookupFails_ReturnsDatabaseError()
    {
        // Arrange
        var request = new OAuthRegisterRequest
        {
            Email = "new@test.com",
            Provider = "Google",
            ProviderToken = "provider-token",
            FullName = "New User",
        };
        _authRepository
            .Setup(findsOAuthLink => findsOAuthLink.FindOAuthLink(request.Provider, request.ProviderToken))
            .Returns(Error.Failure("db_failed", "Database failed."));

        // Act
        ErrorOr<Success> result = _service.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("database_error");
    }

    [Fact]
    public void OAuthRegister_WhenExistingUserLookupFails_ReturnsDatabaseError()
    {
        // Arrange
        var request = new OAuthRegisterRequest
        {
            Email = "new@test.com",
            Provider = "Google",
            ProviderToken = "provider-token",
            FullName = "New User",
        };
        _authRepository
            .Setup(findsOAuthLink => findsOAuthLink.FindOAuthLink(request.Provider, request.ProviderToken))
            .Returns(Error.NotFound());
        _authRepository
            .Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns(Error.Failure("db_failed", "Database failed."));

        // Act
        ErrorOr<Success> result = _service.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("database_error");
    }

    [Fact]
    public void OAuthRegister_WhenUserDoesNotExist_CreatesOAuthOnlyUser()
    {
        // Arrange
        var request = new OAuthRegisterRequest
        {
            Email = "new@test.com",
            Provider = "Google",
            ProviderToken = "provider-token",
            FullName = "New User",
        };
        var savedUser = new User { Id = 7, Email = request.Email };
        _authRepository
            .Setup(findsOAuthLink => findsOAuthLink.FindOAuthLink(request.Provider, request.ProviderToken))
            .Returns(Error.NotFound());
        _authRepository
            .SetupSequence(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns(Error.NotFound())
            .Returns((ErrorOr<User>)savedUser);
        _authRepository
            .Setup(createsUser => createsUser.CreateUser(It.IsAny<User>()))
            .Returns(Result.Success);
        _authRepository
            .Setup(createsOAuthLink => createsOAuthLink.CreateOAuthLink(It.IsAny<OAuthLink>()))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _service.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeFalse();
        _authRepository.Verify(
            createsUser => createsUser.CreateUser(
                It.Is<User>(user =>
                    user.Email == request.Email &&
                    user.FullName == request.FullName &&
                    user.PasswordHash == null &&
                    user.PreferredLanguage == "en" &&
                    !user.Is2FaEnabled &&
                    !user.IsLocked &&
                    user.FailedLoginAttempts == 0)),
            Times.Once);
        _hashService.Verify(getsHash => getsHash.GetHash(It.IsAny<string>()), Times.Never);
    }
}
