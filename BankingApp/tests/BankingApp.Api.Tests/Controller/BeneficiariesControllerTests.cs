namespace BankingApp.Api.Tests.Controller;

using Controllers;
using Application.DTOs.Beneficiaries;
using Application.Repositories.Interfaces;
using Domain.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Trait("Category", "Unit")]
public sealed class BeneficiariesControllerTests
{
    private const int DefaultUserId = 1;
    private const int DefaultBeneficiaryId = 42;

    private readonly Mock<IBeneficiaryRepository> _beneficiaryRepository = new(MockBehavior.Strict);

    [Fact]
    public void GetBeneficiaries_WhenRepositoryReturnsList_ReturnsOkWithMappedDtos()
    {
        var beneficiaries = new List<Beneficiary>
        {
            BuildBeneficiary(DefaultBeneficiaryId, "Alice", "RO49AAAA1B31007593840000"),
            BuildBeneficiary(DefaultBeneficiaryId + 1, "Bob", "RO49AAAA1B31007593840001"),
        };
        _beneficiaryRepository.Setup(repository => repository.FindByUserId(DefaultUserId)).Returns(beneficiaries);
        BeneficiariesController controller = CreateController();

        IActionResult result = controller.GetBeneficiaries();

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        List<BeneficiaryDto> dtos = ok.Value.Should().BeAssignableTo<List<BeneficiaryDto>>().Subject;
        dtos.Should().HaveCount(2);
        dtos[0].Id.Should().Be(DefaultBeneficiaryId);
        dtos[0].Name.Should().Be("Alice");
        _beneficiaryRepository.Verify(repository => repository.FindByUserId(DefaultUserId), Times.Once);
    }

    [Fact]
    public void GetBeneficiaries_WhenRepositoryFails_ReturnsError()
    {
        _beneficiaryRepository.Setup(repository => repository.FindByUserId(DefaultUserId)).Returns(Error.Failure());
        BeneficiariesController controller = CreateController();

        IActionResult result = controller.GetBeneficiaries();

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void GetBeneficiaryById_WhenFound_ReturnsOkWithDto()
    {
        Beneficiary beneficiary = BuildBeneficiary(DefaultBeneficiaryId, "Alice", "RO49AAAA1B31007593840000");
        _beneficiaryRepository.Setup(repository => repository.FindById(DefaultBeneficiaryId, DefaultUserId)).Returns(beneficiary);
        BeneficiariesController controller = CreateController();

        IActionResult result = controller.GetBeneficiaryById(DefaultBeneficiaryId);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        BeneficiaryDto dto = ok.Value.Should().BeOfType<BeneficiaryDto>().Subject;
        dto.Id.Should().Be(DefaultBeneficiaryId);
        dto.Name.Should().Be("Alice");
    }

    [Fact]
    public void GetBeneficiaryById_WhenNotFound_ReturnsNotFound()
    {
        _beneficiaryRepository.Setup(repository => repository.FindById(DefaultBeneficiaryId, DefaultUserId)).Returns(Error.NotFound());
        BeneficiariesController controller = CreateController();

        IActionResult result = controller.GetBeneficiaryById(DefaultBeneficiaryId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void CreateBeneficiary_WhenRequestIsValid_ReturnsOkWithDto()
    {
        var request = new CreateBeneficiaryRequest { Name = "Alice", Iban = "RO49AAAA1B31007593840000", BankName = "BRD" };
        Beneficiary created = BuildBeneficiary(DefaultBeneficiaryId, "Alice", "RO49AAAA1B31007593840000", "BRD");
        _beneficiaryRepository
            .Setup(repository => repository.Create(It.Is<Beneficiary>(beneficiary =>
                beneficiary.Name == "Alice" &&
                beneficiary.Iban == "RO49AAAA1B31007593840000" &&
                beneficiary.BankName == "BRD" &&
                beneficiary.User != null && beneficiary.User.Id == DefaultUserId)))
            .Returns(created);
        BeneficiariesController controller = CreateController();

        IActionResult result = controller.CreateBeneficiary(request);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        BeneficiaryDto dto = ok.Value.Should().BeOfType<BeneficiaryDto>().Subject;
        dto.Id.Should().Be(DefaultBeneficiaryId);
        dto.Name.Should().Be("Alice");
    }

    [Fact]
    public void CreateBeneficiary_WhenRepositoryFails_ReturnsBadRequest()
    {
        var request = new CreateBeneficiaryRequest { Name = "X", Iban = "RO49AAAA1B31007593840000" };
        _beneficiaryRepository.Setup(repository => repository.Create(It.IsAny<Beneficiary>())).Returns(Error.Validation());
        BeneficiariesController controller = CreateController();

        IActionResult result = controller.CreateBeneficiary(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void UpdateBeneficiary_WhenSuccessful_ReturnsNoContent()
    {
        var request = new UpdateBeneficiaryRequest { Name = "Alice Updated", Iban = "RO49AAAA1B31007593840000", BankName = "ING" };
        _beneficiaryRepository
            .Setup(repository => repository.Update(It.Is<Beneficiary>(beneficiary =>
                beneficiary.Id == DefaultBeneficiaryId &&
                beneficiary.User != null && beneficiary.User.Id == DefaultUserId &&
                beneficiary.Name == "Alice Updated")))
            .Returns(Result.Success);
        BeneficiariesController controller = CreateController();

        IActionResult result = controller.UpdateBeneficiary(DefaultBeneficiaryId, request);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void UpdateBeneficiary_WhenNotFound_ReturnsNotFound()
    {
        var request = new UpdateBeneficiaryRequest { Name = "Ghost", Iban = "RO49AAAA1B31007593840099" };
        _beneficiaryRepository.Setup(repository => repository.Update(It.IsAny<Beneficiary>())).Returns(Error.NotFound());
        BeneficiariesController controller = CreateController();

        IActionResult result = controller.UpdateBeneficiary(DefaultBeneficiaryId, request);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void UpdateBeneficiary_UsesRouteIdNotRequestBodyId()
    {
        const int routeId = 99;
        var request = new UpdateBeneficiaryRequest { Id = 1, Name = "Alice", Iban = "RO49AAAA1B31007593840000" };
        _beneficiaryRepository
            .Setup(repository => repository.Update(It.Is<Beneficiary>(beneficiary => beneficiary.Id == routeId)))
            .Returns(Result.Success);
        BeneficiariesController controller = CreateController();

        IActionResult result = controller.UpdateBeneficiary(routeId, request);

        result.Should().BeOfType<NoContentResult>();
        _beneficiaryRepository.Verify(repository => repository.Update(It.Is<Beneficiary>(beneficiary => beneficiary.Id == routeId)), Times.Once);
    }

    [Fact]
    public void DeleteBeneficiary_WhenSuccessful_ReturnsNoContent()
    {
        _beneficiaryRepository.Setup(repository => repository.Delete(DefaultBeneficiaryId, DefaultUserId)).Returns(Result.Success);
        BeneficiariesController controller = CreateController();

        IActionResult result = controller.DeleteBeneficiary(DefaultBeneficiaryId);

        result.Should().BeOfType<NoContentResult>();
        _beneficiaryRepository.Verify(repository => repository.Delete(DefaultBeneficiaryId, DefaultUserId), Times.Once);
    }

    [Fact]
    public void DeleteBeneficiary_WhenNotFound_ReturnsNotFound()
    {
        _beneficiaryRepository.Setup(repository => repository.Delete(DefaultBeneficiaryId, DefaultUserId)).Returns(Error.NotFound());
        BeneficiariesController controller = CreateController();

        IActionResult result = controller.DeleteBeneficiary(DefaultBeneficiaryId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    private static Beneficiary BuildBeneficiary(int id, string name, string iban, string? bankName = null) =>
        new()
        {
            Id = id,
            User = new User { Id = DefaultUserId },
            Name = name,
            Iban = iban,
            BankName = bankName,
            CreatedAt = DateTime.UtcNow,
        };

    private BeneficiariesController CreateController()
    {
        BeneficiariesController controller = new(_beneficiaryRepository.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Items = { ["UserId"] = DefaultUserId } }
            }
        };
        return controller;
    }
}
