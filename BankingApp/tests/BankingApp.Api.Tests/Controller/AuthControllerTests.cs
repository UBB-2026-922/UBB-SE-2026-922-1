namespace BankingApp.Api.Tests.Controller;

using Controllers;
using Application.Repositories.Interfaces;
using Domain.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Trait("Category", "Unit")]
public sealed class AuthControllerTests
{
    private readonly Mock<IAuthRepository> _authRepository = new(MockBehavior.Strict);

    [Fact]
    public void FindUserByEmail_WhenUserExists_ReturnsOkWithUser()
    {
        var user = new User { Id = 1, Email = "user@test.com" };
        _authRepository.Setup(repository => repository.FindUserByEmail("user@test.com")).Returns(user);
        AuthController controller = CreateController();

        IActionResult result = controller.FindUserByEmail("user@test.com");

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(user);
    }

    [Fact]
    public void FindUserByEmail_WhenUserNotFound_ReturnsNotFound()
    {
        _authRepository.Setup(repository => repository.FindUserByEmail("missing@test.com"))
            .Returns(Error.NotFound());
        AuthController controller = CreateController();

        IActionResult result = controller.FindUserByEmail("missing@test.com");

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void FindUserById_WhenUserExists_ReturnsOkWithUser()
    {
        var user = new User { Id = 5 };
        _authRepository.Setup(repository => repository.FindUserById(5)).Returns(user);
        AuthController controller = CreateController();

        IActionResult result = controller.FindUserById(5);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(user);
    }

    [Fact]
    public void FindUserById_WhenUserNotFound_ReturnsNotFound()
    {
        _authRepository.Setup(repository => repository.FindUserById(99)).Returns(Error.NotFound());
        AuthController controller = CreateController();

        IActionResult result = controller.FindUserById(99);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void CreateUser_WhenSuccessful_ReturnsNoContent()
    {
        var user = new User { Email = "new@test.com" };
        _authRepository.Setup(repository => repository.CreateUser(user)).Returns(Result.Success);
        AuthController controller = CreateController();

        IActionResult result = controller.CreateUser(user);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void CreateUser_WhenConflict_ReturnsConflict()
    {
        var user = new User { Email = "dup@test.com" };
        _authRepository.Setup(repository => repository.CreateUser(user)).Returns(Error.Conflict());
        AuthController controller = CreateController();

        IActionResult result = controller.CreateUser(user);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public void FindSessionByToken_WhenFound_ReturnsOkWithSession()
    {
        var session = new Session { Token = "tok" };
        _authRepository.Setup(repository => repository.FindSessionByToken("tok")).Returns(session);
        AuthController controller = CreateController();

        IActionResult result = controller.FindSessionByToken("tok");

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(session);
    }

    [Fact]
    public void IsSessionActive_WhenActive_ReturnsOkTrue()
    {
        _authRepository.Setup(repository => repository.IsSessionActive("tok")).Returns(true);
        AuthController controller = CreateController();

        IActionResult result = controller.IsSessionActive("tok");

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(true);
    }

    [Fact]
    public void RevokeSession_WhenSuccessful_ReturnsNoContent()
    {
        _authRepository.Setup(repository => repository.UpdateSessionToken(7)).Returns(Result.Success);
        AuthController controller = CreateController();

        IActionResult result = controller.RevokeSession(7);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void InvalidateAllSessions_WhenSuccessful_ReturnsNoContent()
    {
        _authRepository.Setup(repository => repository.InvalidateAllSessions(1)).Returns(Result.Success);
        AuthController controller = CreateController();

        IActionResult result = controller.InvalidateAllSessions(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void IncrementFailedAttempts_WhenSuccessful_ReturnsNoContent()
    {
        _authRepository.Setup(repository => repository.IncrementFailedAttempts(1)).Returns(Result.Success);
        AuthController controller = CreateController();

        IActionResult result = controller.IncrementFailedAttempts(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void ResetFailedAttempts_WhenSuccessful_ReturnsNoContent()
    {
        _authRepository.Setup(repository => repository.ResetFailedAttempts(1)).Returns(Result.Success);
        AuthController controller = CreateController();

        IActionResult result = controller.ResetFailedAttempts(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void LockAccount_WhenSuccessful_ReturnsNoContent()
    {
        DateTime lockoutEnd = DateTime.UtcNow.AddMinutes(30);
        _authRepository.Setup(repository => repository.LockAccount(1, lockoutEnd)).Returns(Result.Success);
        AuthController controller = CreateController();

        IActionResult result = controller.LockAccount(1, new AuthController.LockAccountRequest { LockoutEnd = lockoutEnd });

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void UpdatePassword_WhenSuccessful_ReturnsNoContent()
    {
        _authRepository.Setup(repository => repository.UpdatePassword(1, "newHash")).Returns(Result.Success);
        AuthController controller = CreateController();

        IActionResult result = controller.UpdatePassword(1, new AuthController.UpdatePasswordRequest { NewPasswordHash = "newHash" });

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void FindPasswordResetToken_WhenFound_ReturnsOk()
    {
        var token = new PasswordResetToken { TokenHash = "hash" };
        _authRepository.Setup(repository => repository.FindPasswordResetToken("hash")).Returns(token);
        AuthController controller = CreateController();

        IActionResult result = controller.FindPasswordResetToken("hash");

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(token);
    }

    [Fact]
    public void DeleteExpiredPasswordResetTokens_WhenSuccessful_ReturnsNoContent()
    {
        _authRepository.Setup(repository => repository.DeleteExpiredPasswordResetTokens()).Returns(Result.Success);
        AuthController controller = CreateController();

        IActionResult result = controller.DeleteExpiredPasswordResetTokens();

        result.Should().BeOfType<NoContentResult>();
    }

    private AuthController CreateController()
    {
        AuthController controller = new(_authRepository.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        return controller;
    }
}
