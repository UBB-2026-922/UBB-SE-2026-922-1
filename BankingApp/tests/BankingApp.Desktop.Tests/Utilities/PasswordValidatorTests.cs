using BankingApp.Desktop.Utilities;

namespace BankingApp.Desktop.Tests.Utilities;

public class PasswordValidatorTests
{
    [Fact]
    public void MeetsMinimumLength_WhenPasswordHasMinimumLength_ReturnsTrue()
    {
        // Arrange
        string password = new string('A', PasswordValidator.MinimumLength);

        // Act
        bool result = PasswordValidator.MeetsMinimumLength(password);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("1234567")]
    public void MeetsMinimumLength_WhenPasswordIsBlankOrTooShort_ReturnsFalse(string password)
    {
        // Arrange

        // Act
        bool result = PasswordValidator.MeetsMinimumLength(password);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsStrong_WhenPasswordMeetsAllRequirements_ReturnsTrue()
    {
        // Arrange
        string password = "Strong1!";

        // Act
        bool result = PasswordValidator.IsStrong(password);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("weak1!aa")]
    [InlineData("WEAK1!AA")]
    [InlineData("WeakPass!")]
    [InlineData("WeakPass1")]
    [InlineData("Str1!")]
    public void IsStrong_WhenPasswordMissesRequirements_ReturnsFalse(string password)
    {
        // Arrange

        // Act
        bool result = PasswordValidator.IsStrong(password);

        // Assert
        result.Should().BeFalse();
    }
}