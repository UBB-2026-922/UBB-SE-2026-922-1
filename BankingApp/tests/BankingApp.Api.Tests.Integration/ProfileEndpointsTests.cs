// <copyright file="ProfileEndpointsTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

namespace BankingApp.Api.Tests.Integration;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Infrastructure;
using Application.DTOs.Profile;
using ErrorOr;
using FluentAssertions;

public class ProfileEndpointsTests : IClassFixture<BankingAppWebFactory>
{
    private const string ValidToken = "valid-token";
    private const int ValidUserId = 1;

    private readonly HttpClient _client;
    private readonly BankingAppWebFactory _factory;
    private readonly CancellationToken _cancellationToken;

    public ProfileEndpointsTests(BankingAppWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _cancellationToken = TestContext.Current.CancellationToken;

        // Reset mocks before each test to ensure isolated state
        _factory.ProfileServiceMock.Invocations.Clear();

        // Ensure the token validation and session are bypassed
        _factory.JwtServiceMock
            .Setup(jwtService => jwtService.ExtractUserId(ValidToken))
            .Returns(ValidUserId);

        _factory.AuthRepositoryMock
            .Setup(authRepository => authRepository.IsSessionActive(ValidToken))
            .Returns(true);
    }

    [Fact]
    public async Task GetProfile_WhenUserExists_ShouldReturnOkWithProfileInfo()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        var expectedProfile = new ProfileDto
        {
            Email = "user@test.com",
            FullName = "Test User",
        };

        _factory.ProfileServiceMock
            .Setup(profileService => profileService.GetProfile(ValidUserId))
            .Returns(expectedProfile);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ProfileDto? result = await response.Content.ReadFromJsonAsync<ProfileDto>(_cancellationToken);
        result.Should().NotBeNull();
        result!.Email.Should().Be("user@test.com");
    }

    [Fact]
    public async Task UpdateProfile_WhenDataIsValid_ShouldReturnNoContent()
    {
        // Arrange
        var requestData = new UpdateProfileRequest
        {
            UserId = ValidUserId,
            PhoneNumber = "1234567890",
            Address = "123 Test St",
        };

        var request = new HttpRequestMessage(HttpMethod.Put, "/api/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);
        request.Content = JsonContent.Create(requestData);

        _factory.ProfileServiceMock
            .Setup(profileService => profileService.UpdatePersonalInfo(
                It.Is<UpdateProfileRequest>(updateProfileRequest => updateProfileRequest.UserId == ValidUserId)))
            .Returns(Result.Success);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ChangePassword_WhenOldPasswordIsIncorrect_ShouldReturnBadRequest()
    {
        // Arrange
        var requestData = new ChangePasswordRequest
        {
            CurrentPassword = "wrong",
            NewPassword = "newPassword1!",
        };

        var request = new HttpRequestMessage(HttpMethod.Put, "/api/profile/password");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);
        request.Content = JsonContent.Create(requestData);

        _factory.ProfileServiceMock
            .Setup(profileService => profileService.ChangePassword(It.IsAny<ChangePasswordRequest>()))
            .Returns(Error.Validation("Password.Mismatch", "Old password does not match."));

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
