namespace BankingApp.Api.Tests.Controller;

using Controllers;
using Application.Repositories.Interfaces;
using Domain.Entities;
using Domain.Enums;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Trait("Category", "Unit")]
public sealed class TransferControllerTests
{
    private const int DefaultUserId = 1;
    private const int DefaultTransferId = 50;

    private readonly Mock<ITransferRepository> _transferRepository = new(MockBehavior.Strict);

    [Fact]
    public void GetById_WhenFound_ReturnsOkWithTransfer()
    {
        var transfer = new Transfer { Id = DefaultTransferId, Amount = 200m };
        _transferRepository.Setup(repository => repository.GetById(DefaultTransferId)).Returns(transfer);
        TransferController controller = CreateController();

        IActionResult result = controller.GetById(DefaultTransferId);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(transfer);
    }

    [Fact]
    public void GetById_WhenNotFound_ReturnsNotFound()
    {
        _transferRepository.Setup(repository => repository.GetById(99)).Returns(Error.NotFound());
        TransferController controller = CreateController();

        IActionResult result = controller.GetById(99);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void GetByUserId_WhenTransfersExist_ReturnsOkWithList()
    {
        var transfers = new List<Transfer> { new() { Id = 1, Amount = 100m } };
        _transferRepository.Setup(repository => repository.GetByUserId(DefaultUserId)).Returns(transfers);
        TransferController controller = CreateController();

        IActionResult result = controller.GetByUserId(DefaultUserId);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(transfers);
    }

    [Fact]
    public void GetByUserId_WhenRepositoryFails_ReturnsError()
    {
        _transferRepository.Setup(repository => repository.GetByUserId(DefaultUserId)).Returns(Error.Failure());
        TransferController controller = CreateController();

        IActionResult result = controller.GetByUserId(DefaultUserId);

        ObjectResult obj = result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void Create_WhenSuccessful_ReturnsCreatedAtAction()
    {
        var transfer = new Transfer { Amount = 300m, Currency = "RON" };
        var created = new Transfer { Id = DefaultTransferId, Amount = 300m, Currency = "RON" };
        _transferRepository.Setup(repository => repository.Create(transfer)).Returns(created);
        TransferController controller = CreateController();

        IActionResult result = controller.Create(transfer);

        CreatedAtActionResult created201 = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created201.ActionName.Should().Be(nameof(TransferController.GetById));
        created201.Value.Should().Be(created);
    }

    [Fact]
    public void Create_WhenRepositoryFails_ReturnsError()
    {
        var transfer = new Transfer { Amount = -1m };
        _transferRepository.Setup(repository => repository.Create(transfer)).Returns(Error.Validation());
        TransferController controller = CreateController();

        IActionResult result = controller.Create(transfer);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void UpdateStatus_WhenSuccessful_ReturnsOkWithUpdatedTransfer()
    {
        var updated = new Transfer { Id = DefaultTransferId, Status = TransferStatus.Completed };
        _transferRepository.Setup(repository => repository.UpdateStatus(DefaultTransferId, TransferStatus.Completed)).Returns(updated);
        TransferController controller = CreateController();

        IActionResult result = controller.UpdateStatus(DefaultTransferId, TransferStatus.Completed);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(updated);
    }

    [Fact]
    public void UpdateStatus_WhenNotFound_ReturnsNotFound()
    {
        _transferRepository.Setup(repository => repository.UpdateStatus(99, TransferStatus.Completed)).Returns(Error.NotFound());
        TransferController controller = CreateController();

        IActionResult result = controller.UpdateStatus(99, TransferStatus.Completed);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    private TransferController CreateController()
    {
        TransferController controller = new(_transferRepository.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Items = { ["UserId"] = DefaultUserId } }
            }
        };
        return controller;
    }
}
