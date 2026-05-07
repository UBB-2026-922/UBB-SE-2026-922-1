namespace BankingApp.Infrastructure.Tests.Services.Security;

using BankingApp.Infrastructure.Services.Security;
using ErrorOr;

public class HashServiceTests
{
    [Fact]
    public void GetHash_WhenInputIsValid_ReturnsHash()
    {
        // Arrange
        var service = new HashService();

        // Act
        ErrorOr<string> hashResult = service.GetHash("ValidPassword123!");

        // Assert
        hashResult.IsError.Should().BeFalse();
        hashResult.Value.Should().NotBeNullOrWhiteSpace();
        hashResult.Value.Should().NotBe("ValidPassword123!");
    }

    [Fact]
    public void GetHash_WhenInputIsNull_ReturnsFailure()
    {
        // Arrange
        var service = new HashService();

        // Act
        ErrorOr<string> hashResult = service.GetHash(null!);

        // Assert
        hashResult.IsError.Should().BeTrue();
        hashResult.FirstError.Code.Should().Be("hash.failed");
    }

    [Fact]
    public void Verify_WhenInputMatchesHash_ReturnsTrue()
    {
        // Arrange
        var service = new HashService();
        ErrorOr<string> hashResult = service.GetHash("ValidPassword123!");

        // Act
        ErrorOr<bool> verifyResult = service.Verify("ValidPassword123!", hashResult.Value);

        // Assert
        verifyResult.IsError.Should().BeFalse();
        verifyResult.Value.Should().BeTrue();
    }

    [Fact]
    public void Verify_WhenInputDoesNotMatchHash_ReturnsFalse()
    {
        // Arrange
        var service = new HashService();
        ErrorOr<string> hashResult = service.GetHash("ValidPassword123!");

        // Act
        ErrorOr<bool> verifyResult = service.Verify("WrongPassword123!", hashResult.Value);

        // Assert
        verifyResult.IsError.Should().BeFalse();
        verifyResult.Value.Should().BeFalse();
    }

    [Fact]
    public void Verify_WhenHashIsMalformed_ReturnsFailure()
    {
        // Arrange
        var service = new HashService();

        // Act
        ErrorOr<bool> verifyResult = service.Verify("ValidPassword123!", "not-a-bcrypt-hash");

        // Assert
        verifyResult.IsError.Should().BeTrue();
        verifyResult.FirstError.Code.Should().Be("hash.verify_failed");
    }
}