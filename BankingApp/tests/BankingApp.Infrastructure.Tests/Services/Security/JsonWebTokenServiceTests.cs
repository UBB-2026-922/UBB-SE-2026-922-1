namespace BankingApp.Infrastructure.Tests.Services.Security;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BankingApp.Infrastructure.Common.Security;
using ErrorOr;
using Microsoft.IdentityModel.Tokens;

public class JsonWebTokenServiceTests
{
    private const string Secret = "test-secret-key-for-jwt-tests-1234567890";

    [Fact]
    public void GenerateToken_WhenSecretIsValid_ReturnsTokenThatCanBeValidatedAndRead()
    {
        // Arrange
        var service = new JsonWebTokenService(Secret);

        // Act
        ErrorOr<string> tokenResult = service.GenerateToken(42);
        ErrorOr<ClaimsPrincipal> validationResult = service.ValidateToken(tokenResult.Value);
        ErrorOr<int> userIdResult = service.ExtractUserId(tokenResult.Value);

        // Assert
        tokenResult.IsError.Should().BeFalse();
        tokenResult.Value.Should().NotBeNullOrWhiteSpace();
        validationResult.IsError.Should().BeFalse();
        validationResult.Value.FindFirst("userId")?.Value.Should().Be("42");
        userIdResult.IsError.Should().BeFalse();
        userIdResult.Value.Should().Be(42);
    }

    [Fact]
    public void GenerateToken_WhenSecretIsNull_ReturnsFailure()
    {
        // Arrange
        var service = new JsonWebTokenService(null!);

        // Act
        ErrorOr<string> tokenResult = service.GenerateToken(42);

        // Assert
        tokenResult.IsError.Should().BeTrue();
        tokenResult.FirstError.Code.Should().Be("jwt.generate_failed");
    }

    [Fact]
    public void ValidateToken_WhenTokenIsMalformed_ReturnsInvalidTokenError()
    {
        // Arrange
        var service = new JsonWebTokenService(Secret);

        // Act
        ErrorOr<ClaimsPrincipal> validationResult = service.ValidateToken("not-a-token");

        // Assert
        validationResult.IsError.Should().BeTrue();
        validationResult.FirstError.Code.Should().Be("token_invalid");
    }

    [Fact]
    public void ValidateToken_WhenTokenIsExpired_ReturnsExpiredTokenError()
    {
        // Arrange
        var service = new JsonWebTokenService(Secret);
        string token = CreateToken(
            Secret,
            new[] { new Claim("userId", "42") },
            DateTime.UtcNow.AddMinutes(-10));

        // Act
        ErrorOr<ClaimsPrincipal> validationResult = service.ValidateToken(token);

        // Assert
        validationResult.IsError.Should().BeTrue();
        validationResult.FirstError.Code.Should().Be("token_expired");
    }

    [Fact]
    public void ExtractUserId_WhenTokenDoesNotContainUserIdClaim_ReturnsMissingClaimError()
    {
        // Arrange
        var service = new JsonWebTokenService(Secret);
        string token = CreateToken(
            Secret,
            new[] { new Claim("scope", "read") },
            DateTime.UtcNow.AddMinutes(5));

        // Act
        ErrorOr<int> userIdResult = service.ExtractUserId(token);

        // Assert
        userIdResult.IsError.Should().BeTrue();
        userIdResult.FirstError.Code.Should().Be("token_missing_claim");
    }

    private static string CreateToken(string secret, IEnumerable<Claim> claims, DateTime expires)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}