namespace BankingApp.Api.Tests.Controller;

using Controllers;
using Application.Repositories.Interfaces;
using Application.Services.Beneficiary;
using Domain.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Application.DTOs.Beneficiaries;

[Trait("Category", "Unit")]
public sealed class BeneficiariesControllerTests
{
    private const int DefaultUserId = 1;
    private const int DefaultBeneficiaryId = 42;

    private readonly Mock<IBeneficiaryService> _beneficiaryService = new(MockBehavior.Strict);
    private readonly Mock<IBeneficiaryRepository> _beneficiaryRepository = new(MockBehavior.Strict);

    [Fact]
    public void GetBeneficiaries_WhenServiceReturnsList_ReturnsOkWithMappedDtos()
    {
        // Arrange
        var beneficiaries = new List<Beneficiary>
        {
            BuildBeneficiary(DefaultBeneficiaryId, "Alice", "RO49AAAA1B31007593840000"),
            BuildBeneficiary(DefaultBeneficiaryId + 1, "Bob", "RO49AAAA1B31007593840001"),
        };
        _beneficiaryService.Setup(service => service.GetByUserId(DefaultUserId)).Returns(beneficiaries);
        BeneficiariesController controller = CreateController();

        // Act
        IActionResult result = controller.GetBeneficiaries();

        // Assert
        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        List<BeneficiaryDto>? dtos = ok.Value.Should().BeAssignableTo<List<BeneficiaryDto>>().Subject;
        dtos.Should().HaveCount(2);
        dtos[0].Id.Should().Be(DefaultBeneficiaryId);
        dtos[0].Name.Should().Be("Alice");
        _beneficiaryService.Verify(service => service.GetByUserId(DefaultUserId), Times.Once);
    }

    [Fact]
    public void GetBeneficiaries_WhenServiceReturnsError_ReturnsErrorResponse()
    {
        // Arrange
        _beneficiaryService
            .Setup(service => service.GetByUserId(DefaultUserId))
            .Returns(Error.Failure(description: "storage error"));
        BeneficiariesController controller = CreateController();

        // Act
        IActionResult result = controller.GetBeneficiaries();

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void GetBeneficiaryById_WhenBeneficiaryExists_ReturnsOkWithDto()
    {
        // Arrange
        Beneficiary beneficiary = BuildBeneficiary(DefaultBeneficiaryId, "Alice", "RO49AAAA1B31007593840000");
        _beneficiaryService
            .Setup(service => service.GetById(DefaultBeneficiaryId, DefaultUserId))
            .Returns(beneficiary);
        BeneficiariesController controller = CreateController();

        // Act
        IActionResult result = controller.GetBeneficiaryById(DefaultBeneficiaryId);

        // Assert
        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        BeneficiaryDto? dto = ok.Value.Should().BeOfType<BeneficiaryDto>().Subject;
        dto.Id.Should().Be(DefaultBeneficiaryId);
        dto.Name.Should().Be("Alice");
    }

    [Fact]
    public void GetBeneficiaryById_WhenBeneficiaryNotFound_ReturnsNotFound()
    {
        // Arrange
        _beneficiaryService
            .Setup(service => service.GetById(DefaultBeneficiaryId, DefaultUserId))
            .Returns(Error.NotFound(description: "Beneficiary not found"));
        BeneficiariesController controller = CreateController();

        // Act
        IActionResult result = controller.GetBeneficiaryById(DefaultBeneficiaryId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void CreateBeneficiary_WhenRequestIsValid_ReturnsOkWithCreatedDto()
    {
        // Arrange
        var request = new CreateBeneficiaryRequest
        {
            Name = "Alice",
            Iban = "RO49AAAA1B31007593840000",
            BankName = "BRD",
        };
        Beneficiary created = BuildBeneficiary(DefaultBeneficiaryId, request.Name, request.Iban, request.BankName);
        _beneficiaryService
            .Setup(service => service.Create(DefaultUserId, request.Name, request.Iban, request.BankName))
            .Returns(created);
        BeneficiariesController controller = CreateController();

        // Act
        IActionResult result = controller.CreateBeneficiary(request);

        // Assert
        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        BeneficiaryDto? dto = ok.Value.Should().BeOfType<BeneficiaryDto>().Subject;
        dto.Id.Should().Be(DefaultBeneficiaryId);
        dto.Name.Should().Be("Alice");
        dto.BankName.Should().Be("BRD");
    }

    [Fact]
    public void CreateBeneficiary_WhenServiceReturnsValidationError_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateBeneficiaryRequest { Name = string.Empty, Iban = "INVALID" };
        _beneficiaryService
            .Setup(service => service.Create(DefaultUserId, request.Name, request.Iban, request.BankName))
            .Returns(Error.Validation(description: "Invalid IBAN"));
        BeneficiariesController controller = CreateController();

        // Act
        IActionResult result = controller.CreateBeneficiary(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void CreateBeneficiary_WhenIbanAlreadyExists_ReturnsConflict()
    {
        // Arrange
        var request = new CreateBeneficiaryRequest
        {
            Name = "Alice",
            Iban = "RO49AAAA1B31007593840000",
        };
        _beneficiaryService
            .Setup(service => service.Create(DefaultUserId, request.Name, request.Iban, request.BankName))
            .Returns(Error.Conflict(description: "IBAN already saved"));
        BeneficiariesController controller = CreateController();

        // Act
        IActionResult result = controller.CreateBeneficiary(request);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public void UpdateBeneficiary_WhenUpdateSucceeds_ReturnsNoContent()
    {
        // Arrange
        var request = new UpdateBeneficiaryRequest
        {
            Name = "Alice Updated",
            Iban = "RO49AAAA1B31007593840000",
            BankName = "ING",
        };
        _beneficiaryService
            .Setup(service => service.Update(It.Is<Beneficiary>(beneficiary =>
                beneficiary.Id == DefaultBeneficiaryId &&
                beneficiary.User != null &&
                beneficiary.User.Id == DefaultUserId &&
                beneficiary.Name == request.Name &&
                beneficiary.Iban == request.Iban &&
                beneficiary.BankName == request.BankName)))
            .Returns(Result.Success);
        BeneficiariesController controller = CreateController();

        // Act
        IActionResult result = controller.UpdateBeneficiary(DefaultBeneficiaryId, request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void UpdateBeneficiary_WhenBeneficiaryNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateBeneficiaryRequest { Name = "Ghost", Iban = "RO49AAAA1B31007593840099" };
        _beneficiaryService
            .Setup(service => service.Update(It.IsAny<Beneficiary>()))
            .Returns(Error.NotFound(description: "Beneficiary not found"));
        BeneficiariesController controller = CreateController();

        // Act
        IActionResult result = controller.UpdateBeneficiary(DefaultBeneficiaryId, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void UpdateBeneficiary_WhenCalled_UsesRouteIdNotRequestBodyId()
    {
        // Arrange
        const int routeId = 99;
        var request = new UpdateBeneficiaryRequest { Id = 1, Name = "Alice", Iban = "RO49AAAA1B31007593840000" };
        _beneficiaryService
            .Setup(service => service.Update(It.Is<Beneficiary>(beneficiary => beneficiary.Id == routeId)))
            .Returns(Result.Success);
        BeneficiariesController controller = CreateController();

        // Act
        IActionResult result = controller.UpdateBeneficiary(routeId, request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _beneficiaryService.Verify(service => service.Update(It.Is<Beneficiary>(beneficiary => beneficiary.Id == routeId)), Times.Once);
    }

    [Fact]
    public void DeleteBeneficiary_WhenBeneficiaryExists_ReturnsNoContent()
    {
        // Arrange
        _beneficiaryService
            .Setup(service => service.Delete(DefaultBeneficiaryId, DefaultUserId))
            .Returns(Result.Success);
        BeneficiariesController controller = CreateController();

        // Act
        IActionResult result = controller.DeleteBeneficiary(DefaultBeneficiaryId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _beneficiaryService.Verify(service => service.Delete(DefaultBeneficiaryId, DefaultUserId), Times.Once);
    }

    [Fact]
    public void DeleteBeneficiary_WhenBeneficiaryNotFound_ReturnsNotFound()
    {
        // Arrange
        _beneficiaryService
            .Setup(service => service.Delete(DefaultBeneficiaryId, DefaultUserId))
            .Returns(Error.NotFound(description: "Beneficiary not found"));
        BeneficiariesController controller = CreateController();

        // Act
        IActionResult result = controller.DeleteBeneficiary(DefaultBeneficiaryId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void DeleteBeneficiary_WhenCalled_ForwardsAuthenticatedUserIdToService()
    {
        // Arrange
        _beneficiaryService
            .Setup(service => service.Delete(DefaultBeneficiaryId, DefaultUserId))
            .Returns(Result.Success);
        BeneficiariesController controller = CreateController();

        // Act
        controller.DeleteBeneficiary(DefaultBeneficiaryId);

        // Assert
        _beneficiaryService.Verify(service => service.Delete(DefaultBeneficiaryId, DefaultUserId), Times.Once);
    }

    private static Beneficiary BuildBeneficiary(
        int id,
        string name,
        string iban,
        string? bankName = null)
    {
        return new Beneficiary
        {
            Id = id,
            User = new User { Id = DefaultUserId },
            Name = name,
            Iban = iban,
            BankName = bankName,
            CreatedAt = DateTime.UtcNow,
        };
    }

    private BeneficiariesController CreateController()
    {
        var controller = new BeneficiariesController(_beneficiaryService.Object, _beneficiaryRepository.Object);
        var httpContext = new DefaultHttpContext
        {
            Items = { ["UserId"] = DefaultUserId },
        };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }
}
