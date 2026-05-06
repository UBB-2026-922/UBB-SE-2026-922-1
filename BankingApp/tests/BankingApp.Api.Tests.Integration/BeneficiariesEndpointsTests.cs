// <copyright file="BeneficiariesEndpointsTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankingApp.Api.Tests.Integration.Infrastructure;
using BankingApp.Application.DataTransferObjects.Beneficiary;
using BankingApp.Domain.Entities;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace BankingApp.Api.Tests.Integration;

public class BeneficiariesEndpointsTests : IClassFixture<BankingAppWebFactory>
{
    private const string ValidToken = "valid-token";
    private const int ValidUserId = 1;

    private readonly HttpClient _client;
    private readonly BankingAppWebFactory _factory;

    public BeneficiariesEndpointsTests(BankingAppWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // Reset mocks before each test to ensure isolated state
        _factory.BeneficiaryServiceMock.Invocations.Clear();

        // Ensure the token validation and session are bypassed
        _factory.JwtServiceMock
            .Setup(extractsUserId => extractsUserId.ExtractUserId(ValidToken))
            .Returns(ValidUserId);

        _factory.AuthRepositoryMock
            .Setup(auth => auth.IsSessionActive(ValidToken))
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
                UserId = ValidUserId,
                Name = "John Doe",
                Iban = "RO49AAAA1B31007593840000",
                BankName = "Test Bank",
            },
        };

        _factory.BeneficiaryServiceMock
            .Setup(s => s.GetByUserId(ValidUserId))
            .Returns(beneficiaries);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<BeneficiaryDataTransferObject>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result![0].Name.Should().Be("John Doe");
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
            UserId = ValidUserId,
            Name = "Jane Doe",
            Iban = "RO49AAAA1B31007593840001",
            BankName = "Another Bank",
        };

        _factory.BeneficiaryServiceMock
            .Setup(s => s.Create(ValidUserId, "Jane Doe", "RO49AAAA1B31007593840001", "Another Bank"))
            .Returns(createdBeneficiary);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BeneficiaryDataTransferObject>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task DeleteBeneficiary_WhenBeneficiaryDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/beneficiaries/999");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        _factory.BeneficiaryServiceMock
            .Setup(s => s.Delete(999, ValidUserId))
            .Returns(Error.NotFound("Beneficiary.NotFound", "Beneficiary not found."));

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
