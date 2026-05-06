// <copyright file="ExchangeEndpointsTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankingApp.Api.Tests.Integration.Infrastructure;
using BankingApp.Application.DTOs.TeamB;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using ErrorOr;
using FluentAssertions;
using Moq;

namespace BankingApp.Api.Tests.Integration;

public class ExchangeEndpointsTests : IClassFixture<BankingAppWebFactory>
{
    private const string ValidToken = "valid-token";
    private const int ValidUserId = 1;

    private readonly HttpClient _client;
    private readonly BankingAppWebFactory _factory;

    public ExchangeEndpointsTests(BankingAppWebFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // Reset mocks before each test to ensure isolated state
        _factory.ExchangeServiceMock.Invocations.Clear();
        _factory.BillPaymentRepositoryMock.Invocations.Clear();

        // Ensure the token validation and session are bypassed
        _factory.JwtServiceMock
            .Setup(extractsUserId => extractsUserId.ExtractUserId(ValidToken))
            .Returns(ValidUserId);

        _factory.AuthRepositoryMock
            .Setup(auth => auth.IsSessionActive(ValidToken))
            .Returns(true);
    }

    [Fact]
    public async Task GetPreview_WhenCurrencyPairIsValid_ShouldReturnOkWithPreview()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/exchange/preview?sourceCurrency=USD&targetCurrency=EUR&amount=100");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        var preview = new ExchangeTransactionResponseDto
        {
            SourceCurrency = "USD",
            TargetCurrency = "EUR",
            SourceAmount = 100m,
            TargetAmount = 92m,
            ExchangeRate = 0.92m,
        };

        _factory.ExchangeServiceMock
            .Setup(s => s.GetRatePreview("USD", "EUR", 100m))
            .Returns(preview);

        _factory.ExchangeServiceMock
            .Setup(s => s.LockRate(ValidUserId, "USD", "EUR"))
            .Returns(new LockedRate());

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ExchangeTransactionResponseDto>();
        result.Should().NotBeNull();
        result!.SourceCurrency.Should().Be("USD");
        result.TargetCurrency.Should().Be("EUR");
    }

    [Fact]
    public async Task GetHistory_WhenUserHasExchanges_ShouldReturnOkWithList()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/exchange/history");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        var history = new List<ExchangeTransactionResponseDto>
        {
            new ExchangeTransactionResponseDto
            {
                Id = 1,
                SourceCurrency = "USD",
                TargetCurrency = "EUR",
                SourceAmount = 100m,
                TargetAmount = 92m,
                ExchangeRate = 0.92m,
                Status = ExchangeTransactionStatus.Completed,
            },
        };

        _factory.ExchangeServiceMock
            .Setup(s => s.GetExchangeHistory(ValidUserId))
            .Returns(history);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<ExchangeTransactionResponseDto>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPreview_WhenCurrencyPairIsInvalid_ShouldReturnError()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/exchange/preview?sourceCurrency=INVALID&targetCurrency=EUR&amount=100");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        _factory.ExchangeServiceMock
            .Setup(s => s.GetRatePreview("INVALID", "EUR", 100m))
            .Returns(Error.Validation("Currency.Invalid", "The source currency is not supported."));

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
