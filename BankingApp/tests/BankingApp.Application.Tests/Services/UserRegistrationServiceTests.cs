namespace BankingApp.Application.Tests.Services;


using BankingApp.Application.Features.UserRegistration.Services;
using BankingApp.Application.Common.Security;
using Domain.Entities;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

using BankingApp.Application.Features.Authentication.Dtos;

/// <summary>
///     Unit tests for <see cref="UserRegistrationService" />.
/// </summary>
public class UserRegistrationServiceTests
{
    private readonly Mock<IAuthenticationRepository> _authRepository = MockFactory.CreateAuthRepository();
    private readonly Mock<IHashService> _hashService = MockFactory.CreateHashService();
    private readonly UserRegistrationService _service;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserRegistrationServiceTests" /> class.
    /// </summary>
    public UserRegistrationServiceTests()
    {
        _service = new UserRegistrationService(
            _authRepository.Object,
            _hashService.Object,
            NullLogger<UserRegistrationService>.Instance);
    }

    [Fact]
    public void Register_WhenExistingUserLookupFails_ReturnsDatabaseError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "new@test.com",
            Password = "StrongPass1!",
            FullName = "New User"
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
            FullName = "New User"
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
}
