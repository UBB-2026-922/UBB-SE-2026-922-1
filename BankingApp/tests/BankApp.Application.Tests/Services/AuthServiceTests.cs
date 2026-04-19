using BankApp.Application.DataTransferObjects.Auth;
using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Auth;
using BankApp.Application.Services.Notifications;
using BankApp.Application.Services.Security;
using BankApp.Domain.Entities;
using BankApp.Domain.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankApp.Application.Tests.Services;

public sealed class AuthServiceTests
{
    private const int TokenStillValidMinutes = 5;
    private const int TokenAlreadyExpiredMinutes = -5;
    private const int LockoutDurationMinutes = 10;
    private const int AttemptsBeforeLockout = 4;
    private readonly AuthService _authService;
    private readonly Mock<IAuthRepository> _mockAuthRepository = MockFactory.CreateAuthRepository();
    private readonly Mock<IEmailService> _mockEmailService = MockFactory.CreateEmailService();
    private readonly Mock<IHashService> _mockHashService = MockFactory.CreateHashService();
    private readonly Mock<IJsonWebTokenService> _mockJwtService = MockFactory.CreateJwtService();
    private readonly Mock<IOtpService> _mockOtpService = MockFactory.CreateOtpService();

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthServiceTests" /> class.
    /// </summary>
    public AuthServiceTests()
    {
        _authService = new AuthService(
            _mockAuthRepository.Object,
            _mockHashService.Object,
            _mockJwtService.Object,
            _mockOtpService.Object,
            _mockEmailService.Object,
            NullLogger<AuthService>.Instance);
    }

    /// <summary>
    ///     Verifies the Login_WhenEmailIsNotValid_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void Login_WhenEmailIsNotValid_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest { Email = "invalid_email", Password = "invalid_password" };

        // Act
        ErrorOr<LoginSuccess> result = _authService.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("invalid_email");
    }

    /// <summary>
    ///     Verifies the Login_WhenUserNotFound_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void Login_WhenUserNotFound_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest { Email = "fake@user.com", Password = "fake_password" };
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns(Error.Failure());

        // Act
        ErrorOr<LoginSuccess> result = _authService.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        result.FirstError.Code.Should().Be("invalid_credentials");
    }

    /// <summary>
    ///     Verifies the Login_WhenAccountIsLocked_ReturnsForbidden scenario.
    /// </summary>
    [Fact]
    public void Login_WhenAccountIsLocked_ReturnsForbidden()
    {
        // Arrange
        var request = new LoginRequest { Email = "locked@user.com", Password = "password" };
        var user = new User
        {
            Id = 1, Email = request.Email, IsLocked = true,
            LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes),
        };
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)user);

        // Act
        ErrorOr<LoginSuccess> result = _authService.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        result.FirstError.Code.Should().Be("account_locked");
    }

    /// <summary>
    ///     Verifies the Login_WhenPasswordVerificationThrowsError_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void Login_WhenPasswordVerificationThrowsError_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@test.com", Password = "ValidPassword1!" };
        var user = new User { Id = 1, Email = request.Email, PasswordHash = "hash", IsLocked = false };
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)user);
        _mockHashService.Setup(verifies => verifies.Verify(request.Password, user.PasswordHash))
            .Returns(Error.Failure("verify_failed"));

        // Act
        ErrorOr<LoginSuccess> result = _authService.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("verify_failed");
    }

    /// <summary>
    ///     Verifies the Login_WhenPasswordIsWrong_IncrementsFailedAttempts scenario.
    /// </summary>
    [Fact]
    public void Login_WhenPasswordIsWrong_IncrementsFailedAttempts()
    {
        // Arrange
        var request = new LoginRequest { Email = "wrongpass@user.com", Password = "WrongPassword1!" };
        var user = new User { Id = 1, Email = request.Email, PasswordHash = "hash", FailedLoginAttempts = 1 };
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)user);
        _mockHashService.Setup(verifies => verifies.Verify(request.Password, user.PasswordHash)).Returns(false);
        _mockAuthRepository.Setup(incrementsFailedAttempts => incrementsFailedAttempts.IncrementFailedAttempts(user.Id))
            .Returns(Result.Success);

        // Act
        ErrorOr<LoginSuccess> result = _authService.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("invalid_credentials");
        _mockAuthRepository.Verify(
            incrementsFailedAttempts => incrementsFailedAttempts.IncrementFailedAttempts(user.Id),
            Times.Once);
    }

    /// <summary>
    ///     Verifies the Login_WhenPasswordIsWrong_AndMaxAttemptsReached_LockAccountFails_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void Login_WhenPasswordIsWrong_AndMaxAttemptsReached_LockAccountFails_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest { Email = "lockme@user.com", Password = "WrongPassword1!" };
        var user = new User
            { Id = 1, Email = request.Email, PasswordHash = "hash", FailedLoginAttempts = AttemptsBeforeLockout };
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)user);
        _mockHashService.Setup(verifies => verifies.Verify(request.Password, user.PasswordHash)).Returns(false);
        _mockAuthRepository.Setup(incrementsFailedAttempts => incrementsFailedAttempts.IncrementFailedAttempts(user.Id))
            .Returns(Result.Success);
        _mockAuthRepository.Setup(locksAccount => locksAccount.LockAccount(user.Id, It.IsAny<DateTime>()))
            .Returns(Error.Failure("lock_failed"));

        // Act
        ErrorOr<LoginSuccess> result = _authService.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        result.FirstError.Code.Should().Be("invalid_credentials");
    }

    /// <summary>
    ///     Verifies the Login_WhenPasswordIsWrong_AndMaxAttemptsReached_LocksAccount scenario.
    /// </summary>
    [Fact]
    public void Login_WhenPasswordIsWrong_AndMaxAttemptsReached_LocksAccount()
    {
        // Arrange
        var request = new LoginRequest { Email = "lockme@user.com", Password = "WrongPassword1!" };
        var user = new User
            { Id = 1, Email = request.Email, PasswordHash = "hash", FailedLoginAttempts = AttemptsBeforeLockout };
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)user);
        _mockHashService.Setup(verifies => verifies.Verify(request.Password, user.PasswordHash)).Returns(false);
        _mockAuthRepository.Setup(incrementsFailedAttempts => incrementsFailedAttempts.IncrementFailedAttempts(user.Id))
            .Returns(Result.Success);
        _mockAuthRepository.Setup(locksAccount => locksAccount.LockAccount(user.Id, It.IsAny<DateTime>()))
            .Returns(Result.Success);

        // Act
        ErrorOr<LoginSuccess> result = _authService.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Forbidden);
        _mockAuthRepository.Verify(locksAccount => locksAccount.LockAccount(user.Id, It.IsAny<DateTime>()), Times.Once);
        _mockEmailService.Verify(
            sendsLockNotification => sendsLockNotification.SendLockNotification(user.Email),
            Times.Once);
    }

    /// <summary>
    ///     Verifies the Login_When2FAEnabled_AndOtpGenerationFails_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void Login_When2FAEnabled_AndOtpGenerationFails_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest { Email = "2fa@user.com", Password = "ValidPassword123!" };
        var user = new User
        {
            Id = 1, Email = request.Email, PasswordHash = "hash", Is2FaEnabled = true,
            Preferred2FaMethod = TwoFactorMethod.Email,
        };
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)user);
        _mockHashService.Setup(verifies => verifies.Verify(request.Password, user.PasswordHash)).Returns(true);
        _mockOtpService.Setup(generatesTotp => generatesTotp.GenerateTotp(user.Id))
            .Returns((ErrorOr<string>)Error.Failure("otp_failed"));

        // Act
        ErrorOr<LoginSuccess> result = _authService.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("otp_failed");
    }

    /// <summary>
    ///     Verifies the Login_WhenUserHas2FA_ReturnsRequiresTwoFactor scenario.
    /// </summary>
    [Fact]
    public void Login_WhenUserHas2FA_ReturnsRequiresTwoFactor()
    {
        // Arrange
        var request = new LoginRequest { Email = "2fa@user.com", Password = "ValidPassword123!" };
        var user = new User
        {
            Id = 1, Email = request.Email, PasswordHash = "hash", Is2FaEnabled = true,
            Preferred2FaMethod = TwoFactorMethod.Email,
        };
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)user);
        _mockHashService.Setup(verifies => verifies.Verify(request.Password, user.PasswordHash)).Returns(true);
        _mockOtpService.Setup(generatesTotp => generatesTotp.GenerateTotp(user.Id)).Returns((ErrorOr<string>)"123456");

        // Act
        ErrorOr<LoginSuccess> result = _authService.Login(request);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<RequiresTwoFactor>();
        _mockEmailService.Verify(sendsOtpCode => sendsOtpCode.SendOtpCode(user.Email, "123456"), Times.Once);
    }

    /// <summary>
    ///     Verifies the Login_WhenCompleteLogin_AndTokenGenerationFails_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void Login_WhenCompleteLogin_AndTokenGenerationFails_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest { Email = "ok@user.com", Password = "ValidPassword123!" };
        var user = new User { Id = 1, Email = request.Email, PasswordHash = "hash", Is2FaEnabled = false };
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)user);
        _mockHashService.Setup(verifies => verifies.Verify(request.Password, user.PasswordHash)).Returns(true);
        _mockAuthRepository.Setup(resetsFailedAttempts => resetsFailedAttempts.ResetFailedAttempts(user.Id))
            .Returns(Result.Success);
        _mockJwtService.Setup(generatesToken => generatesToken.GenerateToken(user.Id))
            .Returns((ErrorOr<string>)Error.Failure("jwt_failed"));

        // Act
        ErrorOr<LoginSuccess> result = _authService.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("jwt_failed");
    }

    /// <summary>
    ///     Verifies the Login_WhenCompleteLogin_AndSessionCreationFailed_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void Login_WhenCompleteLogin_AndSessionCreationFailed_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest { Email = "ok@user.com", Password = "ValidPassword123!" };
        var user = new User { Id = 1, Email = request.Email, PasswordHash = "hash", Is2FaEnabled = false };
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)user);
        _mockHashService.Setup(verifies => verifies.Verify(request.Password, user.PasswordHash)).Returns(true);
        _mockAuthRepository.Setup(resetsFailedAttempts => resetsFailedAttempts.ResetFailedAttempts(user.Id))
            .Returns(Result.Success);
        _mockJwtService.Setup(generatesToken => generatesToken.GenerateToken(user.Id))
            .Returns((ErrorOr<string>)"jwt-token");
        _mockAuthRepository
            .Setup(createsSession => createsSession.CreateSession(user.Id, "jwt-token", null, null, null))
            .Returns((ErrorOr<Session>)Error.Failure("session_failed"));

        // Act
        ErrorOr<LoginSuccess> result = _authService.Login(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("session_failed");
    }

    /// <summary>
    ///     Verifies the Login_WhenValid_ReturnsFullLogin scenario.
    /// </summary>
    [Fact]
    public void Login_WhenValid_ReturnsFullLogin()
    {
        // Arrange
        var request = new LoginRequest { Email = "ok@user.com", Password = "ValidPassword123!" };
        var user = new User { Id = 1, Email = request.Email, PasswordHash = "hash", Is2FaEnabled = false };
        var token = "jwt-token";
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)user);
        _mockHashService.Setup(verifies => verifies.Verify(request.Password, user.PasswordHash)).Returns(true);
        _mockAuthRepository.Setup(resetsFailedAttempts => resetsFailedAttempts.ResetFailedAttempts(user.Id))
            .Returns(Result.Success);
        _mockJwtService.Setup(generatesToken => generatesToken.GenerateToken(user.Id)).Returns((ErrorOr<string>)token);
        _mockAuthRepository.Setup(createsSession => createsSession.CreateSession(user.Id, token, null, null, null))
            .Returns((ErrorOr<Session>)new Session());

        // Act
        ErrorOr<LoginSuccess> result = _authService.Login(request);

        // Assert
        result.IsError.Should().BeFalse();
        FullLogin? login = result.Value.Should().BeOfType<FullLogin>().Subject;
        login.UserId.Should().Be(user.Id);
        login.Token.Should().Be(token);
    }

    /// <summary>
    ///     Verifies the Register_WhenEmailIsInvalid_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void Register_WhenEmailIsInvalid_ReturnsValidationError()
    {
        // Arrange
        var request = new RegisterRequest { Email = "invalid", Password = "ValidPassword1!", FullName = "Name" };

        // Act
        ErrorOr<Success> result = _authService.Register(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("invalid_email");
    }

    /// <summary>
    ///     Verifies the Register_WhenPasswordIsWeak_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void Register_WhenPasswordIsWeak_ReturnsValidationError()
    {
        // Arrange
        var request = new RegisterRequest { Email = "test@user.com", Password = "weak", FullName = "John Doe" };

        // Act
        ErrorOr<Success> result = _authService.Register(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("weak_password");
    }

    /// <summary>
    ///     Verifies the Register_WhenFullNameIsEmpty_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void Register_WhenFullNameIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var request = new RegisterRequest
            { Email = "test@user.com", Password = "ValidPassword1!", FullName = string.Empty };

        // Act
        ErrorOr<Success> result = _authService.Register(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("full_name_required");
    }

    /// <summary>
    ///     Verifies the Register_WhenEmailAlreadyExists_ReturnsConflict scenario.
    /// </summary>
    [Fact]
    public void Register_WhenEmailAlreadyExists_ReturnsConflict()
    {
        // Arrange
        var request = new RegisterRequest
            { Email = "existing@user.com", Password = "ValidPassword123!", FullName = "John Doe" };
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)new User());

        // Act
        ErrorOr<Success> result = _authService.Register(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("email_registered");
    }

    /// <summary>
    ///     Verifies the Register_WhenHashGenerationFails_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void Register_WhenHashGenerationFails_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest
            { Email = "new@user.com", Password = "ValidPassword123!", FullName = "John Doe" };
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns(Error.NotFound());
        _mockHashService.Setup(getsHash => getsHash.GetHash(request.Password))
            .Returns((ErrorOr<string>)Error.Failure("hash_failed"));

        // Act
        ErrorOr<Success> result = _authService.Register(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("hash_failed");
    }

    /// <summary>
    ///     Verifies the Register_WhenUserCreationFailed_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void Register_WhenUserCreationFailed_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest
            { Email = "new@user.com", Password = "ValidPassword123!", FullName = "John Doe" };
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns(Error.NotFound());
        _mockHashService.Setup(getsHash => getsHash.GetHash(request.Password)).Returns((ErrorOr<string>)"hashed_pass");
        _mockAuthRepository.Setup(createsUser => createsUser.CreateUser(It.IsAny<User>()))
            .Returns(Error.Failure("create_failed"));

        // Act
        ErrorOr<Success> result = _authService.Register(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("user_creation_failed");
    }

    /// <summary>
    ///     Verifies the Register_WhenValid_ReturnsSuccess scenario.
    /// </summary>
    [Fact]
    public void Register_WhenValid_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterRequest
            { Email = "new@user.com", Password = "ValidPassword123!", FullName = "John Doe" };
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns(Error.NotFound());
        _mockHashService.Setup(getsHash => getsHash.GetHash(request.Password)).Returns((ErrorOr<string>)"hashed_pass");
        _mockAuthRepository.Setup(createsUser => createsUser.CreateUser(It.IsAny<User>())).Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _authService.Register(request);

        // Assert
        result.IsError.Should().BeFalse();
        _mockAuthRepository.Verify(
            createsUser => createsUser.CreateUser(
                It.Is<User>(user => user.Email == request.Email && user.FullName == request.FullName)),
            Times.Once);
    }

    /// <summary>
    ///     Verifies the OAuthLoginAsync_WhenProviderIsNotGoogle_ReturnsError scenario.
    /// </summary>
    [Fact]
    public async Task OAuthLoginAsync_WhenProviderIsNotGoogle_ReturnsError()
    {
        // Arrange
        var request = new OAuthLoginRequest { Provider = "Facebook", ProviderToken = "token" };

        // Act
        ErrorOr<LoginSuccess> result = await _authService.OAuthLoginAsync(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("unsupported_provider");
    }

    /// <summary>
    ///     Verifies the OAuthLoginAsync_WhenTokenIsInvalid_ReturnsError scenario.
    /// </summary>
    [Fact]
    public async Task OAuthLoginAsync_WhenTokenIsInvalid_ReturnsError()
    {
        // Arrange
        var request = new OAuthLoginRequest { Provider = "Google", ProviderToken = "invalid_token" };

        // Act
        ErrorOr<LoginSuccess> result = await _authService.OAuthLoginAsync(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("invalid_google_token");
    }

    /// <summary>
    ///     Verifies the OAuthRegister_WhenEmailIsInvalid_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void OAuthRegister_WhenEmailIsInvalid_ReturnsError()
    {
        // Arrange
        var request = new OAuthRegisterRequest
            { Email = "invalid", Provider = "Google", ProviderToken = "token", FullName = "Name" };

        // Act
        ErrorOr<Success> result = _authService.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("invalid_email");
    }

    /// <summary>
    ///     Verifies the OAuthRegister_WhenOAuthLinkExists_ReturnsConflict scenario.
    /// </summary>
    [Fact]
    public void OAuthRegister_WhenOAuthLinkExists_ReturnsConflict()
    {
        // Arrange
        var request = new OAuthRegisterRequest
            { Email = "test@test.com", Provider = "Google", ProviderToken = "token", FullName = "Name" };
        _mockAuthRepository
            .Setup(findsOAuthLink => findsOAuthLink.FindOAuthLink(request.Provider, request.ProviderToken))
            .Returns((ErrorOr<OAuthLink>)new OAuthLink());

        // Act
        ErrorOr<Success> result = _authService.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("oauth_already_registered");
    }

    /// <summary>
    ///     Verifies the OAuthRegister_WhenUserExists_AndCreateLinkFails_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void OAuthRegister_WhenUserExists_AndCreateLinkFails_ReturnsError()
    {
        // Arrange
        var request = new OAuthRegisterRequest
            { Email = "test@test.com", Provider = "Google", ProviderToken = "token", FullName = "Name" };
        var user = new User { Id = 1, Email = request.Email };
        _mockAuthRepository
            .Setup(findsOAuthLink => findsOAuthLink.FindOAuthLink(request.Provider, request.ProviderToken))
            .Returns(Error.NotFound());
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)user);
        _mockAuthRepository.Setup(createsOAuthLink => createsOAuthLink.CreateOAuthLink(It.IsAny<OAuthLink>()))
            .Returns(Error.Failure("link_failed"));

        // Act
        ErrorOr<Success> result = _authService.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("oauth_link_failed");
    }

    /// <summary>
    ///     Verifies the OAuthRegister_WhenUserExists_AndCreateLinkSucceeds_ReturnsSuccess scenario.
    /// </summary>
    [Fact]
    public void OAuthRegister_WhenUserExists_AndCreateLinkSucceeds_ReturnsSuccess()
    {
        // Arrange
        var request = new OAuthRegisterRequest
            { Email = "test@test.com", Provider = "Google", ProviderToken = "token", FullName = "Name" };
        var user = new User { Id = 1, Email = request.Email };
        _mockAuthRepository
            .Setup(findsOAuthLink => findsOAuthLink.FindOAuthLink(request.Provider, request.ProviderToken))
            .Returns(Error.NotFound());
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)user);
        _mockAuthRepository.Setup(createsOAuthLink => createsOAuthLink.CreateOAuthLink(It.IsAny<OAuthLink>()))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _authService.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeFalse();
    }

    /// <summary>
    ///     Verifies the OAuthRegister_WhenUserDoesNotExist_AndHashGenerationFails_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void OAuthRegister_WhenUserDoesNotExist_AndHashGenerationFails_ReturnsError()
    {
        // Arrange
        var request = new OAuthRegisterRequest
            { Email = "test@test.com", Provider = "Google", ProviderToken = "token", FullName = "Name" };
        _mockAuthRepository
            .Setup(findsOAuthLink => findsOAuthLink.FindOAuthLink(request.Provider, request.ProviderToken))
            .Returns(Error.NotFound());
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)Error.NotFound());
        _mockHashService.Setup(getsHash => getsHash.GetHash(It.IsAny<string>()))
            .Returns((ErrorOr<string>)Error.Failure("hash_failed"));

        // Act
        ErrorOr<Success> result = _authService.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("hash_failed");
    }

    /// <summary>
    ///     Verifies the OAuthRegister_WhenUserDoesNotExist_AndCreateUserFails_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void OAuthRegister_WhenUserDoesNotExist_AndCreateUserFails_ReturnsError()
    {
        // Arrange
        var request = new OAuthRegisterRequest
            { Email = "test@test.com", Provider = "Google", ProviderToken = "token", FullName = "Name" };
        _mockAuthRepository
            .Setup(findsOAuthLink => findsOAuthLink.FindOAuthLink(request.Provider, request.ProviderToken))
            .Returns(Error.NotFound());
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)Error.NotFound());
        _mockHashService.Setup(getsHash => getsHash.GetHash(It.IsAny<string>())).Returns((ErrorOr<string>)"hash");
        _mockAuthRepository.Setup(createsUser => createsUser.CreateUser(It.IsAny<User>()))
            .Returns(Error.Failure("create_user_failed"));

        // Act
        ErrorOr<Success> result = _authService.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("user_creation_failed");
    }

    /// <summary>
    ///     Verifies the OAuthRegister_WhenUserDoesNotExist_AndUserRetrievalFails_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void OAuthRegister_WhenUserDoesNotExist_AndUserRetrievalFails_ReturnsError()
    {
        // Arrange
        var request = new OAuthRegisterRequest
            { Email = "test@test.com", Provider = "Google", ProviderToken = "token", FullName = "Name" };
        _mockAuthRepository
            .Setup(findsOAuthLink => findsOAuthLink.FindOAuthLink(request.Provider, request.ProviderToken))
            .Returns(Error.NotFound());
        _mockAuthRepository.SetupSequence(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)Error.NotFound())
            .Returns((ErrorOr<User>)Error.Failure("retrieval_failed"));
        _mockHashService.Setup(getsHash => getsHash.GetHash(It.IsAny<string>())).Returns((ErrorOr<string>)"hash");
        _mockAuthRepository.Setup(createsUser => createsUser.CreateUser(It.IsAny<User>())).Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _authService.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("user_retrieval_failed");
    }

    /// <summary>
    ///     Verifies the OAuthRegister_WhenUserDoesNotExist_AndCreateLinkSucceeds_ReturnsSuccess scenario.
    /// </summary>
    [Fact]
    public void OAuthRegister_WhenUserDoesNotExist_AndCreateLinkSucceeds_ReturnsSuccess()
    {
        // Arrange
        var request = new OAuthRegisterRequest
            { Email = "test@test.com", Provider = "Google", ProviderToken = "token", FullName = "Name" };
        var user = new User { Id = 1, Email = request.Email };
        _mockAuthRepository
            .Setup(findsOAuthLink => findsOAuthLink.FindOAuthLink(request.Provider, request.ProviderToken))
            .Returns(Error.NotFound());
        _mockAuthRepository.SetupSequence(findsUserByEmail => findsUserByEmail.FindUserByEmail(request.Email))
            .Returns((ErrorOr<User>)Error.NotFound())
            .Returns((ErrorOr<User>)user);
        _mockHashService.Setup(getsHash => getsHash.GetHash(It.IsAny<string>())).Returns((ErrorOr<string>)"hash");
        _mockAuthRepository.Setup(createsUser => createsUser.CreateUser(It.IsAny<User>())).Returns(Result.Success);
        _mockAuthRepository.Setup(createsOAuthLink => createsOAuthLink.CreateOAuthLink(It.IsAny<OAuthLink>()))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _authService.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeFalse();
    }

    /// <summary>
    ///     Verifies the VerifyOTP_WhenUserNotFound_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void VerifyOTP_WhenUserNotFound_ReturnsError()
    {
        // Arrange
        var request = new VerifyOtpRequest { UserId = 1, OtpCode = "123456" };
        _mockAuthRepository.Setup(findsUserById => findsUserById.FindUserById(request.UserId))
            .Returns(Error.NotFound("user_not_found"));

        // Act
        ErrorOr<LoginSuccess> result = _authService.VerifyOtp(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("user_not_found");
    }

    /// <summary>
    ///     Verifies the VerifyOTP_WhenVerifyTOTPFails_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void VerifyOTP_WhenVerifyTOTPFails_ReturnsError()
    {
        // Arrange
        var request = new VerifyOtpRequest { UserId = 1, OtpCode = "123456" };
        var user = new User { Id = 1 };
        _mockAuthRepository.Setup(findsUserById => findsUserById.FindUserById(request.UserId))
            .Returns((ErrorOr<User>)user);
        _mockOtpService.Setup(verifiesTotp => verifiesTotp.VerifyTotp(request.UserId, request.OtpCode))
            .Returns(Error.Failure("totp_failed"));

        // Act
        ErrorOr<LoginSuccess> result = _authService.VerifyOtp(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("totp_failed");
    }

    /// <summary>
    ///     Verifies the VerifyOTP_WhenOtpInvalid_ReturnsUnauthorized scenario.
    /// </summary>
    [Fact]
    public void VerifyOTP_WhenOtpInvalid_ReturnsUnauthorized()
    {
        // Arrange
        var request = new VerifyOtpRequest { UserId = 1, OtpCode = "000000" };
        var user = new User { Id = 1 };
        _mockAuthRepository.Setup(findsUserById => findsUserById.FindUserById(request.UserId))
            .Returns((ErrorOr<User>)user);
        _mockOtpService.Setup(verifiesTotp => verifiesTotp.VerifyTotp(request.UserId, request.OtpCode)).Returns(false);

        // Act
        ErrorOr<LoginSuccess> result = _authService.VerifyOtp(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("invalid_otp");
    }

    /// <summary>
    ///     Verifies the VerifyOTP_WhenValid_ReturnsFullLogin scenario.
    /// </summary>
    [Fact]
    public void VerifyOTP_WhenValid_ReturnsFullLogin()
    {
        // Arrange
        var request = new VerifyOtpRequest { UserId = 1, OtpCode = "123456" };
        var user = new User { Id = 1, Email = "test@user.com" };
        _mockAuthRepository.Setup(findsUserById => findsUserById.FindUserById(request.UserId))
            .Returns((ErrorOr<User>)user);
        _mockOtpService.Setup(verifiesTotp => verifiesTotp.VerifyTotp(request.UserId, request.OtpCode)).Returns(true);
        _mockAuthRepository.Setup(resetsFailedAttempts => resetsFailedAttempts.ResetFailedAttempts(user.Id))
            .Returns(Result.Success);
        _mockJwtService.Setup(generatesToken => generatesToken.GenerateToken(user.Id))
            .Returns((ErrorOr<string>)"token");
        _mockAuthRepository.Setup(createsSession => createsSession.CreateSession(user.Id, "token", null, null, null))
            .Returns((ErrorOr<Session>)new Session());

        // Act
        ErrorOr<LoginSuccess> result = _authService.VerifyOtp(request);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeOfType<FullLogin>();
        _mockOtpService.Verify(invalidatesOtp => invalidatesOtp.InvalidateOtp(user.Id), Times.Once);
    }

    /// <summary>
    ///     Verifies the ResendOTP_WhenUserNotFound_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void ResendOTP_WhenUserNotFound_ReturnsError()
    {
        // Arrange
        _mockAuthRepository.Setup(findsUserById => findsUserById.FindUserById(1))
            .Returns((ErrorOr<User>)Error.NotFound("user_not_found"));

        // Act
        ErrorOr<Success> result = _authService.ResendOtp(1, "Email");

        // Assert
        result.IsError.Should().BeTrue();
    }

    /// <summary>
    ///     Verifies the ResendOTP_WhenGenerateTOTPFails_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void ResendOTP_WhenGenerateTOTPFails_ReturnsError()
    {
        // Arrange
        var user = new User { Id = 1 };
        _mockAuthRepository.Setup(findsUserById => findsUserById.FindUserById(1)).Returns((ErrorOr<User>)user);
        _mockOtpService.Setup(generatesTotp => generatesTotp.GenerateTotp(1)).Returns(Error.Failure("totp_failed"));

        // Act
        ErrorOr<Success> result = _authService.ResendOtp(1, "Email");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("totp_failed");
    }

    /// <summary>
    ///     Verifies the ResendOTP_WhenValid_SendsEmailAndReturnsSuccess scenario.
    /// </summary>
    [Fact]
    public void ResendOTP_WhenValid_SendsEmailAndReturnsSuccess()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@user.com", Preferred2FaMethod = TwoFactorMethod.Email };
        _mockAuthRepository.Setup(findsUserById => findsUserById.FindUserById(user.Id)).Returns((ErrorOr<User>)user);
        _mockOtpService.Setup(generatesTotp => generatesTotp.GenerateTotp(user.Id)).Returns((ErrorOr<string>)"123456");

        // Act
        ErrorOr<Success> result = _authService.ResendOtp(user.Id, "Email");

        // Assert
        result.IsError.Should().BeFalse();
        _mockEmailService.Verify(sendsOtpCode => sendsOtpCode.SendOtpCode(user.Email, "123456"), Times.Once);
    }

    /// <summary>
    ///     Verifies the ResendOTP_WhenValidAndMethodIsNotEmail_ReturnsSuccessWithoutEmail scenario.
    /// </summary>
    [Fact]
    public void ResendOTP_WhenValidAndMethodIsNotEmail_ReturnsSuccessWithoutEmail()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@test.com", Preferred2FaMethod = TwoFactorMethod.Phone };
        _mockAuthRepository.Setup(findsUserById => findsUserById.FindUserById(user.Id)).Returns((ErrorOr<User>)user);
        _mockOtpService.Setup(generatesTotp => generatesTotp.GenerateTotp(user.Id)).Returns((ErrorOr<string>)"123456");

        // Act
        ErrorOr<Success> result = _authService.ResendOtp(user.Id, "SMS");

        // Assert
        result.IsError.Should().BeFalse();
        _mockEmailService.Verify(
            sendsOtpCode => sendsOtpCode.SendOtpCode(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    /// <summary>
    ///     Verifies the RequestPasswordReset_WhenUserNotFound_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void RequestPasswordReset_WhenUserNotFound_ReturnsError()
    {
        // Arrange
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail("test@test.com"))
            .Returns((ErrorOr<User>)Error.NotFound("user_not_found"));

        // Act
        ErrorOr<Success> result = _authService.RequestPasswordReset("test@test.com");

        // Assert
        result.IsError.Should().BeTrue();
    }

    /// <summary>
    ///     Verifies the RequestPasswordReset_WhenSaveTokenFails_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void RequestPasswordReset_WhenSaveTokenFails_ReturnsError()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@test.com" };
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(user.Email))
            .Returns((ErrorOr<User>)user);
        _mockAuthRepository
            .Setup(savesPasswordResetToken =>
                savesPasswordResetToken.SavePasswordResetToken(It.IsAny<PasswordResetToken>()))
            .Returns(Error.Failure("save_failed"));

        // Act
        ErrorOr<Success> result = _authService.RequestPasswordReset(user.Email);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Description.Should().Be("Failed to save password reset token.");
    }

    /// <summary>
    ///     Verifies the RequestPasswordReset_WhenUserFound_SendsEmailAndReturnsSuccess scenario.
    /// </summary>
    [Fact]
    public void RequestPasswordReset_WhenUserFound_SendsEmailAndReturnsSuccess()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@test.com" };
        _mockAuthRepository.Setup(findsUserByEmail => findsUserByEmail.FindUserByEmail(user.Email))
            .Returns((ErrorOr<User>)user);
        _mockAuthRepository
            .Setup(deletesExpiredPasswordResetTokens =>
                deletesExpiredPasswordResetTokens.DeleteExpiredPasswordResetTokens()).Returns(Result.Success);
        _mockAuthRepository
            .Setup(savesPasswordResetToken =>
                savesPasswordResetToken.SavePasswordResetToken(It.IsAny<PasswordResetToken>())).Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _authService.RequestPasswordReset(user.Email);

        // Assert
        result.IsError.Should().BeFalse();
        _mockEmailService.Verify(
            sendsPasswordResetLink => sendsPasswordResetLink.SendPasswordResetLink(
                It.Is<string>(email => email == user.Email),
                It.IsAny<string>()),
            Times.Once);
    }

    /// <summary>
    ///     Verifies the ResetPassword_WhenTokenIsNullOrWhiteSpace_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void ResetPassword_WhenTokenIsNullOrWhiteSpace_ReturnsError()
    {
        // Arrange & Act
        ErrorOr<Success> result = _authService.ResetPassword(string.Empty, "ValidPassword123!");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("token_invalid");
    }

    /// <summary>
    ///     Verifies the ResetPassword_WhenTokenNotFound_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void ResetPassword_WhenTokenNotFound_ReturnsError()
    {
        // Arrange
        _mockAuthRepository
            .Setup(findsPasswordResetToken => findsPasswordResetToken.FindPasswordResetToken(It.IsAny<string>()))
            .Returns((ErrorOr<PasswordResetToken>)Error.NotFound("token_not_found"));

        // Act
        ErrorOr<Success> result = _authService.ResetPassword("token", "ValidPassword123!");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("token_invalid");
    }

    /// <summary>
    ///     Verifies the ResetPassword_WhenTokenUsed_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void ResetPassword_WhenTokenUsed_ReturnsError()
    {
        // Arrange
        var token = new PasswordResetToken { UsedAt = DateTime.UtcNow };
        _mockAuthRepository
            .Setup(findsPasswordResetToken => findsPasswordResetToken.FindPasswordResetToken(It.IsAny<string>()))
            .Returns((ErrorOr<PasswordResetToken>)token);

        // Act
        ErrorOr<Success> result = _authService.ResetPassword("token", "ValidPassword123!");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("token_already_used");
    }

    /// <summary>
    ///     Verifies the ResetPassword_WhenTokenExpired_ReturnsValidationError scenario.
    /// </summary>
    [Fact]
    public void ResetPassword_WhenTokenExpired_ReturnsValidationError()
    {
        // Arrange
        var token = new PasswordResetToken
            { UserId = 1, ExpiresAt = DateTime.UtcNow.AddMinutes(TokenAlreadyExpiredMinutes) };
        _mockAuthRepository
            .Setup(findsPasswordResetToken => findsPasswordResetToken.FindPasswordResetToken(It.IsAny<string>()))
            .Returns((ErrorOr<PasswordResetToken>)token);

        // Act
        ErrorOr<Success> result = _authService.ResetPassword("raw_token", "NewValidPassword123!");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("token_expired");
    }

    /// <summary>
    ///     Verifies the ResetPassword_WhenHashGenerationFails_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void ResetPassword_WhenHashGenerationFails_ReturnsError()
    {
        // Arrange
        var token = new PasswordResetToken { ExpiresAt = DateTime.UtcNow.AddMinutes(TokenStillValidMinutes) };
        _mockAuthRepository
            .Setup(findsPasswordResetToken => findsPasswordResetToken.FindPasswordResetToken(It.IsAny<string>()))
            .Returns((ErrorOr<PasswordResetToken>)token);
        _mockHashService.Setup(getsHash => getsHash.GetHash(It.IsAny<string>())).Returns(Error.Failure("hash_failed"));

        // Act
        ErrorOr<Success> result = _authService.ResetPassword("token", "ValidPassword123!");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("hash_failed");
    }

    /// <summary>
    ///     Verifies the ResetPassword_WhenUpdatePasswordFails_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void ResetPassword_WhenUpdatePasswordFails_ReturnsError()
    {
        // Arrange
        var token = new PasswordResetToken
            { UserId = 1, ExpiresAt = DateTime.UtcNow.AddMinutes(TokenStillValidMinutes) };
        _mockAuthRepository
            .Setup(findsPasswordResetToken => findsPasswordResetToken.FindPasswordResetToken(It.IsAny<string>()))
            .Returns((ErrorOr<PasswordResetToken>)token);
        _mockHashService.Setup(getsHash => getsHash.GetHash(It.IsAny<string>())).Returns((ErrorOr<string>)"hash");
        _mockAuthRepository.Setup(updatesPassword => updatesPassword.UpdatePassword(token.UserId, "hash"))
            .Returns(Error.Failure("update_failed"));

        // Act
        ErrorOr<Success> result = _authService.ResetPassword("token", "ValidPassword123!");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("token_invalid");
    }

    /// <summary>
    ///     Verifies the ResetPassword_WhenMarkTokenAsUsedFails_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void ResetPassword_WhenMarkTokenAsUsedFails_ReturnsError()
    {
        // Arrange
        var token = new PasswordResetToken
            { Id = 1, UserId = 1, ExpiresAt = DateTime.UtcNow.AddMinutes(TokenStillValidMinutes) };
        _mockAuthRepository
            .Setup(findsPasswordResetToken => findsPasswordResetToken.FindPasswordResetToken(It.IsAny<string>()))
            .Returns((ErrorOr<PasswordResetToken>)token);
        _mockHashService.Setup(getsHash => getsHash.GetHash(It.IsAny<string>())).Returns((ErrorOr<string>)"new_hash");
        _mockAuthRepository.Setup(updatesPassword => updatesPassword.UpdatePassword(token.UserId, "new_hash"))
            .Returns(Result.Success);
        _mockAuthRepository
            .Setup(marksPasswordResetTokenAsUsed =>
                marksPasswordResetTokenAsUsed.MarkPasswordResetTokenAsUsed(token.Id))
            .Returns(Error.Failure("mark_failed"));

        // Act
        ErrorOr<Success> result = _authService.ResetPassword("raw_token", "ValidPassword123!");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("reset_failed");
    }

    /// <summary>
    ///     Verifies the ResetPassword_WhenInvalidateSessionsFails_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void ResetPassword_WhenInvalidateSessionsFails_ReturnsError()
    {
        // Arrange
        var token = new PasswordResetToken
            { Id = 1, UserId = 1, ExpiresAt = DateTime.UtcNow.AddMinutes(TokenStillValidMinutes) };
        _mockAuthRepository
            .Setup(findsPasswordResetToken => findsPasswordResetToken.FindPasswordResetToken(It.IsAny<string>()))
            .Returns((ErrorOr<PasswordResetToken>)token);
        _mockHashService.Setup(getsHash => getsHash.GetHash(It.IsAny<string>())).Returns((ErrorOr<string>)"new_hash");
        _mockAuthRepository.Setup(updatesPassword => updatesPassword.UpdatePassword(token.UserId, "new_hash"))
            .Returns(Result.Success);
        _mockAuthRepository
            .Setup(marksPasswordResetTokenAsUsed =>
                marksPasswordResetTokenAsUsed.MarkPasswordResetTokenAsUsed(token.Id)).Returns(Result.Success);
        _mockAuthRepository.Setup(invalidatesAllSessions => invalidatesAllSessions.InvalidateAllSessions(token.UserId))
            .Returns(Error.Failure("invalidate_failed"));

        // Act
        ErrorOr<Success> result = _authService.ResetPassword("raw_token", "ValidPassword123!");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("reset_failed");
    }

    /// <summary>
    ///     Verifies the ResetPassword_WhenValid_UpdatesPasswordAndReturnsSuccess scenario.
    /// </summary>
    [Fact]
    public void ResetPassword_WhenValid_UpdatesPasswordAndReturnsSuccess()
    {
        // Arrange
        var token = new PasswordResetToken
            { Id = 1, UserId = 1, ExpiresAt = DateTime.UtcNow.AddMinutes(TokenStillValidMinutes), UsedAt = null };
        var newPassword = "NewValidPassword123!";
        _mockAuthRepository
            .Setup(findsPasswordResetToken => findsPasswordResetToken.FindPasswordResetToken(It.IsAny<string>()))
            .Returns((ErrorOr<PasswordResetToken>)token);
        _mockHashService.Setup(getsHash => getsHash.GetHash(newPassword)).Returns((ErrorOr<string>)"new_hash");
        _mockAuthRepository.Setup(updatesPassword => updatesPassword.UpdatePassword(token.UserId, "new_hash"))
            .Returns(Result.Success);
        _mockAuthRepository
            .Setup(marksPasswordResetTokenAsUsed =>
                marksPasswordResetTokenAsUsed.MarkPasswordResetTokenAsUsed(token.Id)).Returns(Result.Success);
        _mockAuthRepository.Setup(invalidatesAllSessions => invalidatesAllSessions.InvalidateAllSessions(token.UserId))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _authService.ResetPassword("raw_token", newPassword);

        // Assert
        result.IsError.Should().BeFalse();
        _mockAuthRepository.Verify(
            updatesPassword => updatesPassword.UpdatePassword(token.UserId, "new_hash"),
            Times.Once);
        _mockAuthRepository.Verify(
            invalidatesAllSessions => invalidatesAllSessions.InvalidateAllSessions(token.UserId),
            Times.Once);
    }

    /// <summary>
    ///     Verifies the VerifyResetToken_WhenTokenIsNullOrWhiteSpace_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void VerifyResetToken_WhenTokenIsNullOrWhiteSpace_ReturnsError()
    {
        // Arrange & Act
        ErrorOr<Success> result = _authService.VerifyResetToken(string.Empty);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("token_invalid");
    }

    /// <summary>
    ///     Verifies the VerifyResetToken_WhenTokenNotFound_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void VerifyResetToken_WhenTokenNotFound_ReturnsError()
    {
        // Arrange
        _mockAuthRepository
            .Setup(findsPasswordResetToken => findsPasswordResetToken.FindPasswordResetToken(It.IsAny<string>()))
            .Returns((ErrorOr<PasswordResetToken>)Error.NotFound("not_found"));

        // Act
        ErrorOr<Success> result = _authService.VerifyResetToken("token");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("token_invalid");
    }

    /// <summary>
    ///     Verifies the VerifyResetToken_WhenTokenValid_ReturnsSuccess scenario.
    /// </summary>
    [Fact]
    public void VerifyResetToken_WhenTokenValid_ReturnsSuccess()
    {
        // Arrange
        var token = new PasswordResetToken { ExpiresAt = DateTime.UtcNow.AddMinutes(TokenStillValidMinutes) };
        _mockAuthRepository
            .Setup(findsPasswordResetToken => findsPasswordResetToken.FindPasswordResetToken(It.IsAny<string>()))
            .Returns((ErrorOr<PasswordResetToken>)token);

        // Act
        ErrorOr<Success> result = _authService.VerifyResetToken("token");

        // Assert
        result.IsError.Should().BeFalse();
    }

    /// <summary>
    ///     Verifies the Logout_WhenSessionNotFound_ReturnsError scenario.
    /// </summary>
    [Fact]
    public void Logout_WhenSessionNotFound_ReturnsError()
    {
        // Arrange
        _mockAuthRepository.Setup(findsSessionByToken => findsSessionByToken.FindSessionByToken("invalid"))
            .Returns((ErrorOr<Session>)Error.NotFound());

        // Act
        ErrorOr<Success> result = _authService.Logout("invalid");

        // Assert
        result.IsError.Should().BeTrue();
    }

    /// <summary>
    ///     Verifies the Logout_WhenValid_ReturnsSuccess scenario.
    /// </summary>
    [Fact]
    public void Logout_WhenValid_ReturnsSuccess()
    {
        // Arrange
        var session = new Session { Id = 1, UserId = 1 };
        _mockAuthRepository.Setup(findsSessionByToken => findsSessionByToken.FindSessionByToken("valid_token"))
            .Returns((ErrorOr<Session>)session);
        _mockAuthRepository.Setup(updatesSessionToken => updatesSessionToken.UpdateSessionToken(session.Id))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _authService.Logout("valid_token");

        // Assert
        result.IsError.Should().BeFalse();
        _mockAuthRepository.Verify(
            updatesSessionToken => updatesSessionToken.UpdateSessionToken(session.Id),
            Times.Once);
    }
}