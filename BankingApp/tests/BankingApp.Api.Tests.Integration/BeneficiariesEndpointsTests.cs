// <copyright file="BeneficiariesEndpointsTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

namespace BankingApp.Api.Tests.Integration;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Infrastructure;
using Application.DTOs.Beneficiaries;
using Domain.Entities;
using ErrorOr;
using FluentAssertions;

public class BeneficiariesEndpointsTests : IClassFixture<BankingAppWebFactory>
{
    private const string ValidToken = "valid-token";
    private const int ValidUserId = 1;

    private readonly HttpClient _client;
    private readonly BankingAppWebFactory _factory;
    private readonly CancellationToken _cancellationToken;

    public BeneficiariesEndpointsTests(BankingAppWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _cancellationToken = TestContext.Current.CancellationToken;

        // Reset mocks before each test to ensure isolated state
        _factory.BeneficiaryServiceMock.Invocations.Clear();

        // Ensure the token validation and session are bypassed
        _factory.JwtServiceMock
            .Setup(jwtService => jwtService.ExtractUserId(ValidToken))
            .Returns(ValidUserId);

        _factory.AuthRepositoryMock
            .Setup(authRepository => authRepository.IsSessionActive(ValidToken))
            .Returns(true);
    }

    [Fact]
    public async Task GetBeneficiaries_WhenUserHasBeneficiaries_ShouldReturnOkWithList()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/beneficiaries");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        var beneficiaries = new List<Beneficiary>
        {
            new Beneficiary
            {
                Id = 1,
                User = new User { Id = ValidUserId },
                Name = "John Doe",
                Iban = "RO49AAAA1B31007593840000",
                BankName = "Test Bank",
            },
        };

        _factory.BeneficiaryServiceMock
            .Setup(service => service.GetByUserId(ValidUserId))
            .Returns(beneficiaries);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<BeneficiaryDto>? result = await response.Content.ReadFromJsonAsync<List<BeneficiaryDto>>(_cancellationToken);
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task CreateBeneficiary_WhenDataIsValid_ShouldReturnOkWithCreatedBeneficiary()
    {
        // Arrange
        var requestData = new CreateBeneficiaryRequest
        {
            Name = "Jane Doe",
            Iban = "RO49AAAA1B31007593840001",
            BankName = "Another Bank",
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/beneficiaries");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);
        request.Content = JsonContent.Create(requestData);

        var createdBeneficiary = new Beneficiary
        {
            Id = 2,
            User = new User { Id = ValidUserId },
            Name = "Jane Doe",
            Iban = "RO49AAAA1B31007593840001",
            BankName = "Another Bank",
        };

        _factory.BeneficiaryServiceMock
            .Setup(service => service.Create(ValidUserId, "Jane Doe", "RO49AAAA1B31007593840001", "Another Bank"))
            .Returns(createdBeneficiary);

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        BeneficiaryDto? result = await response.Content.ReadFromJsonAsync<BeneficiaryDto>(_cancellationToken);
        result.Should().NotBeNull();
        result.Name.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task DeleteBeneficiary_WhenBeneficiaryDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/beneficiaries/999");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        _factory.BeneficiaryServiceMock
            .Setup(beneficiaryService => beneficiaryService.Delete(999, ValidUserId))
            .Returns(Error.NotFound("Beneficiary.NotFound", "Beneficiary not found."));

        // Act
        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
