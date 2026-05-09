namespace BankingApp.Application.Tests.Services;

using BankingApp.Application.Features.Billers.Commands;
using BankingApp.Application.Features.Billers.Dtos;
using BankingApp.Application.Features.Billers.Queries;
using ErrorOr;

public sealed class GetBillersQueryHandlerTests
{
    private readonly Mock<IBillerRepository> _billerRepo = MockFactory.CreateBillerRepository();

    private GetBillersQueryHandler CreateHandler() => new(_billerRepo.Object);

    [Fact]
    public async Task Handle_WhenNoBillers_ReturnsEmptyList()
    {
        var query = new GetBillersQuery();

        ErrorOr<List<BillerDto>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenBillersExist_ReturnsMappedList()
    {
        var biller = new Biller { Id = 1, Name = "Water Co", Category = BillerCategory.Utilities, IsActive = true };
        _billerRepo.Setup(r => r.ListActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Biller>)new[] { biller });
        var query = new GetBillersQuery();

        ErrorOr<List<BillerDto>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value[0].Name.Should().Be("Water Co");
    }
}

public sealed class GetSavedBillersQueryHandlerTests
{
    private readonly Mock<ISavedBillerRepository> _savedBillerRepo = MockFactory.CreateSavedBillerRepository();
    private readonly Mock<IBillerRepository> _billerRepo = MockFactory.CreateBillerRepository();

    private GetSavedBillersQueryHandler CreateHandler() => new(
        _savedBillerRepo.Object,
        _billerRepo.Object);

    [Fact]
    public async Task Handle_WhenNoSavedBillers_ReturnsEmptyList()
    {
        var query = new GetSavedBillersQuery(1);

        ErrorOr<List<SavedBillerDto>> result = await CreateHandler().Handle(query, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }
}

public sealed class SaveBillerCommandHandlerTests
{
    private readonly Mock<IBillerRepository> _billerRepo = MockFactory.CreateBillerRepository();
    private readonly Mock<ISavedBillerRepository> _savedBillerRepo = MockFactory.CreateSavedBillerRepository();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();
    private readonly Mock<ISystemClock> _clock = MockFactory.CreateSystemClock();

    private SaveBillerCommandHandler CreateHandler() => new(
        _billerRepo.Object,
        _savedBillerRepo.Object,
        _unitOfWork.Object,
        _clock.Object);

    [Fact]
    public async Task Handle_WhenBillerNotFound_ReturnsBillerNotFoundError()
    {
        var command = new SaveBillerCommand(1, 99, "My Biller", null);

        ErrorOr<SavedBillerDto> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenBillerAlreadySaved_ReturnsConflictError()
    {
        var biller = new Biller { Id = 1, Name = "Water Co", Category = BillerCategory.Utilities };
        var existingSaved = SavedBiller.Create(1, 1, null, null, DateTime.UtcNow);
        _billerRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(biller);
        _savedBillerRepo.Setup(r => r.ListByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<SavedBiller>)new[] { existingSaved });
        var command = new SaveBillerCommand(1, 1, "My Biller", null);

        ErrorOr<SavedBillerDto> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValid_SavesBillerAndSaves()
    {
        var biller = new Biller { Id = 1, Name = "Water Co", Category = BillerCategory.Utilities };
        _billerRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(biller);
        var command = new SaveBillerCommand(1, 1, "My Water", null);

        ErrorOr<SavedBillerDto> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.BillerName.Should().Be("Water Co");
        _savedBillerRepo.Verify(r => r.AddAsync(It.IsAny<SavedBiller>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

public sealed class DeleteSavedBillerCommandHandlerTests
{
    private readonly Mock<ISavedBillerRepository> _savedBillerRepo = MockFactory.CreateSavedBillerRepository();
    private readonly Mock<IUnitOfWork> _unitOfWork = MockFactory.CreateUnitOfWork();

    private DeleteSavedBillerCommandHandler CreateHandler() => new(
        _savedBillerRepo.Object,
        _unitOfWork.Object);

    [Fact]
    public async Task Handle_WhenSavedBillerNotFound_ReturnsNotFoundError()
    {
        var command = new DeleteSavedBillerCommand(1, 99);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenSavedBillerBelongsToDifferentUser_ReturnsNotFoundError()
    {
        var saved = SavedBiller.Create(2, 1, null, null, DateTime.UtcNow);
        _savedBillerRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(saved);
        var command = new DeleteSavedBillerCommand(1, 10);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValid_DeletesAndSaves()
    {
        var saved = SavedBiller.Create(1, 1, null, null, DateTime.UtcNow);
        _savedBillerRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(saved);
        var command = new DeleteSavedBillerCommand(1, 10);

        ErrorOr<Success> result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        _savedBillerRepo.Verify(r => r.DeleteAsync(It.IsAny<SavedBiller>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
