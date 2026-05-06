// <copyright file="DashboardEndpointsTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankingApp.Api.Tests.Integration.Infrastructure;
using BankingApp.Application.DataTransferObjects.Dashboard;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace BankingApp.Api.Tests.Integration;

public class DashboardEndpointsTests : IClassFixture<BankingAppWebFactory>
{
    private const string ValidToken = "valid-token";
    private const int ValidUserId = 1;

    private readonly HttpClient _client;
    private readonly BankingAppWebFactory _factory;

    public DashboardEndpointsTests(BankingAppWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // Reset mocks before each test to ensure isolated state
        _factory.DashboardServiceMock.Invocations.Clear();

        // Ensure the token validation and session are bypassed
        _factory.JwtServiceMock
            .Setup(extractsUserId => extractsUserId.ExtractUserId(ValidToken))
            .Returns(ValidUserId);

        _factory.AuthRepositoryMock
            .Setup(auth => auth.IsSessionActive(ValidToken))
            .Returns(true);
    }

    [Fact]
    public async Task GetDashboard_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/dashboard");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        _factory.DashboardServiceMock
            .Setup(s => s.GetDashboardData(ValidUserId))
            .Returns(Error.NotFound("User.NotFound", "The user could not be found."));

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDashboard_WhenUserExists_ShouldReturnOkWithDashboardData()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/dashboard");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        var expectedResponse = new DashboardResponse
        {
            UnreadNotificationCount = 5,
        };

        _factory.DashboardServiceMock
            .Setup(s => s.GetDashboardData(ValidUserId))
            .Returns(expectedResponse);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<DashboardResponse>();
        result.Should().NotBeNull();
        result!.UnreadNotificationCount.Should().Be(5);
    }
}
