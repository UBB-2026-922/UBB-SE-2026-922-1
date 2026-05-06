// <copyright file="ProfileEndpointsTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankingApp.Api.Tests.Integration.Infrastructure;
using BankingApp.Application.DataTransferObjects.Profile;
using ErrorOr;
using FluentAssertions;
using Moq;
using Xunit;

namespace BankingApp.Api.Tests.Integration;

public class ProfileEndpointsTests : IClassFixture<BankingAppWebFactory>
{
    private const string ValidToken = "valid-token";
    private const int ValidUserId = 1;

    private readonly HttpClient _client;
    private readonly BankingAppWebFactory _factory;

    public ProfileEndpointsTests(BankingAppWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // Reset mocks before each test to ensure isolated state
        _factory.ProfileServiceMock.Invocations.Clear();

        // Ensure the token validation and session are bypassed
        _factory.JwtServiceMock
            .Setup(extractsUserId => extractsUserId.ExtractUserId(ValidToken))
            .Returns(ValidUserId);

        _factory.AuthRepositoryMock
            .Setup(auth => auth.IsSessionActive(ValidToken))
            .Returns(true);
    }

    [Fact]
    public async Task GetProfile_WhenUserExists_ShouldReturnOkWithProfileInfo()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        var expectedProfile = new ProfileInfo
        {
            Email = "user@test.com",
            FullName = "Test User",
        };

        _factory.ProfileServiceMock
            .Setup(s => s.GetProfile(ValidUserId))
            .Returns(expectedProfile);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ProfileInfo>();
        result.Should().NotBeNull();
        result!.Email.Should().Be("user@test.com");
    }

    [Fact]
    public async Task UpdateProfile_WhenDataIsValid_ShouldReturnNoContent()
    {
        // Arrange
        var requestData = new UpdateProfileRequest(ValidUserId, "1234567890", "123 Test St");

        var request = new HttpRequestMessage(HttpMethod.Put, "/api/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);
        request.Content = JsonContent.Create(requestData);

        _factory.ProfileServiceMock
            .Setup(s => s.UpdatePersonalInfo(It.Is<UpdateProfileRequest>(r => r.UserId == ValidUserId)))
            .Returns(Result.Success);

        // Act
        var response = await _client.SendAsync(request);

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
            .Setup(s => s.ChangePassword(It.IsAny<ChangePasswordRequest>()))
            .Returns(Error.Validation("Password.Mismatch", "Old password does not match."));

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
