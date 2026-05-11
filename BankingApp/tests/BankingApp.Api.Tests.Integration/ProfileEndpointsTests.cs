namespace BankingApp.Api.Tests.Integration;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankingApp.Api.Tests.Integration.Infrastructure;
using BankingApp.Application.Features.UserProfile.Commands;
using BankingApp.Application.Features.UserProfile.Dtos;
using BankingApp.Application.Features.UserProfile.Queries;
using BankingApp.Domain.Aggregates.IdentityAggregate;

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
    public async Task GetProfile_WhenUserExists_ShouldReturnOkWithProfileInfo()
    {
        _factory.SenderMock
            .Setup(sender => sender.Send(It.IsAny<GetProfileQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProfileDto
            {
                UserId = ValidUserId,
                Email = "user@test.com",
                FullName = "Test User"
            });

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ProfileDto? result = await response.Content.ReadFromJsonAsync<ProfileDto>(_cancellationToken);
        result.Should().NotBeNull();
        result!.Email.Should().Be("user@test.com");
    }

    [Fact]
    public async Task UpdateProfile_WhenDataIsValid_ShouldReturnNoContent()
    {
        _factory.SenderMock
            .Setup(sender => sender.Send(It.IsAny<UpdateProfileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var request = new HttpRequestMessage(HttpMethod.Put, "/api/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);
        request.Content = JsonContent.Create(new UpdateProfileRequest
        {
            PhoneNumber = "1234567890",
            Address = "123 Test St"
        });

        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ChangePassword_WhenOldPasswordIsIncorrect_ShouldReturnBadRequest()
    {
        _factory.SenderMock
            .Setup(sender => sender.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Validation("Password.Mismatch", "Old password does not match."));

        var request = new HttpRequestMessage(HttpMethod.Put, "/api/profile/password");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);
        request.Content = JsonContent.Create(new ChangePasswordRequest
        {
            CurrentPassword = "wrong",
            NewPassword = "newPassword1!"
        });

        HttpResponseMessage response = await _client.SendAsync(request, _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static IdentityAccount CreateActiveIdentity(int userId, string token)
    {
        IdentityAccount identity = IdentityAccount.Create(userId, null);
        identity.OpenSession(token, DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow);
        return identity;
    }
}
