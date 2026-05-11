namespace BankingApp.Api.Tests.Integration;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankingApp.Api.Tests.Integration.Infrastructure;
using BankingApp.Application.Common.Contracts.Security;
using BankingApp.Application.Features.Beneficiaries.Commands;
using BankingApp.Application.Features.Beneficiaries.Dtos;
using BankingApp.Application.Features.Beneficiaries.Queries;
using BankingApp.Domain.Aggregates.IdentityAggregate;
using BankingApp.Domain.Repositories;

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

        _factory.SenderMock.Reset();
        _factory.JwtServiceMock.Reset();
        _factory.IdentityRepositoryMock.Reset();

        _factory.JwtServiceMock
            .Setup(jwtService => jwtService.ExtractUserId(ValidToken))
            .Returns(ValidUserId);

        _factory.IdentityRepositoryMock
            .Setup(repository => repository.GetBySessionTokenAsync(ValidToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateActiveIdentity(ValidUserId, ValidToken));
    }

    [Fact]
    public async Task GetBeneficiaries_WhenUserHasBeneficiaries_ShouldReturnOkWithList()
    {
        _factory.SenderMock
            .Setup(sender => sender.Send(It.IsAny<GetBeneficiariesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BeneficiaryDto>
            {
                new()
                {
                    Id = 1,
                    UserId = ValidUserId,
                    Name = "John Doe",
                    Iban = "RO49AAAA1B31007593840000",
                    BankName = "Test Bank"
                }
            });

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/beneficiaries");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<BeneficiaryDto>? result = await response.Content.ReadFromJsonAsync<List<BeneficiaryDto>>(_cancellationToken);
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result![0].Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task CreateBeneficiary_WhenDataIsValid_ShouldReturnOkWithCreatedBeneficiary()
    {
        _factory.SenderMock
            .Setup(sender => sender.Send(It.IsAny<CreateBeneficiaryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BeneficiaryDto
            {
                Id = 2,
                UserId = ValidUserId,
                Name = "Jane Doe",
                Iban = "RO49AAAA1B31007593840001",
                BankName = "Another Bank"
            });

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/beneficiaries");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);
        request.Content = JsonContent.Create(new CreateBeneficiaryRequest
        {
            Name = "Jane Doe",
            Iban = "RO49AAAA1B31007593840001",
            BankName = "Another Bank"
        });

        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        BeneficiaryDto? result = await response.Content.ReadFromJsonAsync<BeneficiaryDto>(_cancellationToken);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task DeleteBeneficiary_WhenBeneficiaryDoesNotExist_ShouldReturnNotFound()
    {
        _factory.SenderMock
            .Setup(sender => sender.Send(It.IsAny<DeleteBeneficiaryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.NotFound("Beneficiary.NotFound", "Beneficiary not found."));

        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/beneficiaries/999");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static IdentityAccount CreateActiveIdentity(int userId, string token)
    {
        var identity = IdentityAccount.Create(userId, null);
        identity.OpenSession(token, DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow);
        return identity;
    }
}
