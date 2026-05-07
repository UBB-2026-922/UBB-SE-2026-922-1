using BankingApp.Application.DTOs.Profile;
using BankingApp.Desktop.Enums;
using BankingApp.Desktop.Services;
using BankingApp.Desktop.Utilities;
using BankingApp.Desktop.ViewModels;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankingApp.Desktop.Tests.ViewModels;

public class PersonalInfoViewModelTests
{
    private readonly Mock<IProfileClientService> _profileClientService = new(MockBehavior.Strict);

    [Fact]
    public async Task LoadProfile_WhenApiReturnsProfile_PopulatesProfileAndSetsSuccessState()
    {
        // Arrange
        const int userId = 7;
        const string fullName = "Test User";
        const string phoneNumber = "0712345678";
        var profile = new ProfileDto
        {
            UserId = userId,
            FullName = fullName,
            PhoneNumber = phoneNumber,
            Email = "test@example.com",
        };

        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance);

        _profileClientService
            .Setup(s => s.GetProfileAsync())
            .ReturnsAsync(profile);

        // Act
        bool success = await viewModel.LoadProfile();

        // Assert
        success.Should().BeTrue();
        viewModel.ProfileInfo.Should().BeSameAs(profile);
        viewModel.State.Value.Should().Be(ProfileState.UpdateSuccess);
        viewModel.HasPhoneNumber.Should().BeTrue();
        viewModel.TwoFactorPhoneDisplay.Should().Be(phoneNumber);
    }

    [Fact]
    public async Task LoadProfile_WhenApiFails_SetsErrorStateAndLeavesDefaultProfile()
    {
        // Arrange
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance);

        _profileClientService
            .Setup(s => s.GetProfileAsync())
            .ReturnsAsync(Error.Failure(description: "server down"));

        // Act
        bool success = await viewModel.LoadProfile();

        // Assert
        success.Should().BeFalse();
        viewModel.State.Value.Should().Be(ProfileState.Error);
        viewModel.ProfileInfo.UserId.Should().BeNull();
        viewModel.HasPhoneNumber.Should().BeFalse();
        viewModel.TwoFactorPhoneDisplay.Should().Be(UserMessages.Profile.NoPhoneNumber);
    }

    [Fact]
    public async Task UpdatePersonalInfo_WhenUserIdIsNull_SetsErrorState()
    {
        // Arrange
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance);

        // Act
        bool success = await viewModel.UpdatePersonalInfo("0712345678", "123 Main St", "password");

        // Assert
        success.Should().BeFalse();
        viewModel.State.Value.Should().Be(ProfileState.Error);
    }

    [Fact]
    public async Task UpdatePersonalInfo_WhenApiSucceeds_TrimsInputPreservesExistingFieldsAndUpdatesProfile()
    {
        // Arrange
        const int userId = 7;
        DateTime dateOfBirth = new(1999, 12, 31);
        UpdateProfileRequest? sentRequest = null;
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance);
        viewModel.ProfileInfo.UserId = userId;
        viewModel.ProfileInfo.FullName = "Existing Name";
        viewModel.ProfileInfo.PhoneNumber = "0000";
        viewModel.ProfileInfo.Address = "Old Address";
        viewModel.ProfileInfo.DateOfBirth = dateOfBirth;
        viewModel.ProfileInfo.Nationality = "Romanian";
        viewModel.ProfileInfo.PreferredLanguage = "ro";

        _profileClientService
            .Setup(s => s.UpdateProfileAsync(It.IsAny<UpdateProfileRequest>()))
            .Callback<UpdateProfileRequest>(request => sentRequest = request)
            .ReturnsAsync(Result.Success);

        // Act
        bool success = await viewModel.UpdatePersonalInfo(" 0712345678 ", " 123 Main St ", "password", "  Updated Name ");

        // Assert
        success.Should().BeTrue();
        sentRequest.Should().NotBeNull();
        sentRequest!.UserId.Should().Be(userId);
        sentRequest.PhoneNumber.Should().Be("0712345678");
        sentRequest.Address.Should().Be("123 Main St");
        sentRequest.FullName.Should().Be("Updated Name");
        sentRequest.DateOfBirth.Should().Be(dateOfBirth);
        sentRequest.Nationality.Should().Be("Romanian");
        sentRequest.PreferredLanguage.Should().Be("ro");
        viewModel.ProfileInfo.FullName.Should().Be("Updated Name");
        viewModel.ProfileInfo.PhoneNumber.Should().Be("0712345678");
        viewModel.ProfileInfo.Address.Should().Be("123 Main St");
        viewModel.State.Value.Should().Be(ProfileState.UpdateSuccess);
    }

    [Fact]
    public async Task UpdatePersonalInfo_WhenOptionalInputsAreBlank_UsesNullsAndKeepsExistingFullName()
    {
        // Arrange
        UpdateProfileRequest? sentRequest = null;
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance);
        viewModel.ProfileInfo.UserId = 7;
        viewModel.ProfileInfo.FullName = "Existing Name";
        viewModel.ProfileInfo.PhoneNumber = "0711";
        viewModel.ProfileInfo.Address = "Old Address";

        _profileClientService
            .Setup(s => s.UpdateProfileAsync(It.IsAny<UpdateProfileRequest>()))
            .Callback<UpdateProfileRequest>(request => sentRequest = request)
            .ReturnsAsync(Result.Success);

        // Act
        bool success = await viewModel.UpdatePersonalInfo("   ", "   ", "password", "   ");

        // Assert
        success.Should().BeTrue();
        sentRequest.Should().NotBeNull();
        sentRequest!.PhoneNumber.Should().BeNull();
        sentRequest.Address.Should().BeNull();
        sentRequest.FullName.Should().Be("Existing Name");
        viewModel.ProfileInfo.PhoneNumber.Should().BeNull();
        viewModel.ProfileInfo.Address.Should().BeNull();
        viewModel.ProfileInfo.FullName.Should().Be("Existing Name");
        viewModel.State.Value.Should().Be(ProfileState.UpdateSuccess);
    }

    [Fact]
    public async Task UpdatePersonalInfo_WhenApiFails_DoesNotMutateProfileAndSetsErrorState()
    {
        // Arrange
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance);
        viewModel.ProfileInfo.UserId = 7;
        viewModel.ProfileInfo.FullName = "Existing Name";
        viewModel.ProfileInfo.PhoneNumber = "0711";
        viewModel.ProfileInfo.Address = "Old Address";

        _profileClientService
            .Setup(s => s.UpdateProfileAsync(It.IsAny<UpdateProfileRequest>()))
            .ReturnsAsync(Error.Failure(description: "update failed"));

        // Act
        bool success = await viewModel.UpdatePersonalInfo("0722", "New Address", "password", "Updated Name");

        // Assert
        success.Should().BeFalse();
        viewModel.ProfileInfo.FullName.Should().Be("Existing Name");
        viewModel.ProfileInfo.PhoneNumber.Should().Be("0711");
        viewModel.ProfileInfo.Address.Should().Be("Old Address");
        viewModel.State.Value.Should().Be(ProfileState.Error);
    }

    [Fact]
    public async Task VerifyPassword_WhenUserIdIsNull_SetsErrorState()
    {
        // Arrange
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance);

        // Act
        bool success = await viewModel.VerifyPassword("password");

        // Assert
        success.Should().BeFalse();
        viewModel.State.Value.Should().Be(ProfileState.Error);
    }

    [Fact]
    public async Task VerifyPassword_WhenApiReturnsTrue_SetsSuccessState()
    {
        // Arrange
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance);
        viewModel.ProfileInfo.UserId = 7;

        _profileClientService
            .Setup(s => s.VerifyPasswordAsync("correct-password"))
            .ReturnsAsync(true);

        // Act
        bool success = await viewModel.VerifyPassword("correct-password");

        // Assert
        success.Should().BeTrue();
        viewModel.State.Value.Should().Be(ProfileState.UpdateSuccess);
    }

    [Fact]
    public async Task VerifyPassword_WhenApiReturnsFalse_SetsErrorState()
    {
        // Arrange
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance);
        viewModel.ProfileInfo.UserId = 7;

        _profileClientService
            .Setup(s => s.VerifyPasswordAsync("wrong-password"))
            .ReturnsAsync(false);

        // Act
        bool success = await viewModel.VerifyPassword("wrong-password");

        // Assert
        success.Should().BeFalse();
        viewModel.State.Value.Should().Be(ProfileState.Error);
    }

    [Fact]
    public async Task VerifyPassword_WhenApiFails_SetsErrorState()
    {
        // Arrange
        var viewModel = new PersonalInfoViewModel(_profileClientService.Object, NullLogger<PersonalInfoViewModel>.Instance);
        viewModel.ProfileInfo.UserId = 7;

        _profileClientService
            .Setup(s => s.VerifyPasswordAsync("password"))
            .ReturnsAsync(Error.Failure(description: "verification failed"));

        // Act
        bool success = await viewModel.VerifyPassword("password");

        // Assert
        success.Should().BeFalse();
        viewModel.State.Value.Should().Be(ProfileState.Error);
    }
}
