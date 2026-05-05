// <copyright file="BillerServiceTests.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>

using BankingApp.Application.DTOs.Billers;
using BankingApp.Application.Repositories.Interfaces;
using BankingApp.Application.Services.Billers;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Errors;
using ErrorOr;

namespace BankingApp.Application.Tests.Services;

/// <summary>
///     Unit tests for <see cref="BillerService" />.
/// </summary>
public class BillerServiceTests
{
    private const int UserId = 1;
    private const int BillerId = 10;
    private const int SavedBillerId = 100;

    private readonly Mock<IBillerRepository> _billerRepository = new(MockBehavior.Strict);
    private readonly BillerService _service;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BillerServiceTests" /> class.
    /// </summary>
    public BillerServiceTests()
    {
        _service = new BillerService(_billerRepository.Object);
    }

    // GetBillerDirectory tests.

    /// <summary>
    ///     Verifies that GetBillerDirectory returns a mapped DTO list when billers exist.
    /// </summary>
    [Fact]
    public void GetBillerDirectory_WhenBillersExist_ReturnsMappedDtos()
    {
        // Arrange
        var billers = new List<Biller>
        {
            new() { Id = 1, Name = "Water Co", Category = "Utilities", IsActive = true },
            new() { Id = 2, Name = "Electric Co", Category = "Utilities", IsActive = true },
        };
        _billerRepository.Setup(repository => repository.GetAllBillers(true)).Returns(billers);

        // Act
        ErrorOr<List<BillerDataTransferObject>> result = _service.GetBillerDirectory();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value[0].Name.Should().Be("Water Co");
    }

    /// <summary>
    ///     Verifies that GetBillerDirectory returns an empty list when there are no active billers.
    /// </summary>
    [Fact]
    public void GetBillerDirectory_WhenNoBillers_ReturnsEmptyList()
    {
        // Arrange
        _billerRepository.Setup(repository => repository.GetAllBillers(true)).Returns(new List<Biller>());

        // Act
        ErrorOr<List<BillerDataTransferObject>> result = _service.GetBillerDirectory();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    // SearchBillers tests.

    /// <summary>
    ///     Verifies that SearchBillers returns filtered DTOs for a matching search term.
    /// </summary>
    [Fact]
    public void SearchBillers_WithMatchingTerm_ReturnsMappedDtos()
    {
        // Arrange
        var billers = new List<Biller>
        {
            new() { Id = 1, Name = "Water Co", Category = "Utilities", IsActive = true },
        };
        _billerRepository.Setup(repository => repository.SearchBillers("Water", null, true)).Returns(billers);

        // Act
        ErrorOr<List<BillerDataTransferObject>> result = _service.SearchBillers("Water");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle(biller => biller.Name == "Water Co");
    }

    /// <summary>
    ///     Verifies that SearchBillers passes the category filter to the repository.
    /// </summary>
    [Fact]
    public void SearchBillers_WithCategoryFilter_PassesCategoryToRepository()
    {
        // Arrange
        _billerRepository
            .Setup(repository => repository.SearchBillers("Co", "Utilities", true))
            .Returns(new List<Biller>());

        // Act
        ErrorOr<List<BillerDataTransferObject>> result = _service.SearchBillers("Co", "Utilities");

        // Assert
        result.IsError.Should().BeFalse();
        _billerRepository.Verify(repository => repository.SearchBillers("Co", "Utilities", true), Times.Once);
    }

    // GetSavedBillers tests.

    /// <summary>
    ///     Verifies that GetSavedBillers returns mapped DTOs for a user with saved billers.
    /// </summary>
    [Fact]
    public void GetSavedBillers_WhenSavedBillersExist_ReturnsMappedDtos()
    {
        // Arrange
        var biller = new Biller { Id = BillerId, Name = "Gas Co", Category = "Utilities" };
        var saved = new List<SavedBiller>
        {
            new() { Id = SavedBillerId, UserId = UserId, BillerId = BillerId, Nickname = "Home Gas", Biller = biller, CreatedAt = DateTime.UtcNow },
        };
        _billerRepository.Setup(repository => repository.GetSavedBillers(UserId)).Returns(saved);

        // Act
        ErrorOr<List<SavedBillerDataTransferObject>> result = _service.GetSavedBillers(UserId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle(savedBiller => savedBiller.Nickname == "Home Gas" && savedBiller.BillerName == "Gas Co");
    }

    /// <summary>
    ///     Verifies that GetSavedBillers returns an empty list when the user has no saved billers.
    /// </summary>
    [Fact]
    public void GetSavedBillers_WhenNoneSaved_ReturnsEmptyList()
    {
        // Arrange
        _billerRepository.Setup(repository => repository.GetSavedBillers(UserId)).Returns(new List<SavedBiller>());

        // Act
        ErrorOr<List<SavedBillerDataTransferObject>> result = _service.GetSavedBillers(UserId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    // SaveBiller tests.

    /// <summary>
    ///     Verifies that SaveBiller returns the created DTO when the biller is valid and not already saved.
    /// </summary>
    [Fact]
    public void SaveBiller_WhenValidAndNotAlreadySaved_ReturnsCreatedDto()
    {
        // Arrange
        var biller = new Biller { Id = BillerId, Name = "Internet Co", Category = "Telecoms" };
        var request = new SaveBillerRequest { BillerId = BillerId, Nickname = "Home Internet" };
        _billerRepository.Setup(repository => repository.GetBillerById(BillerId)).Returns(biller);
        _billerRepository.Setup(repository => repository.GetSavedBillers(UserId)).Returns(new List<SavedBiller>());
        _billerRepository
            .Setup(repository => repository.SaveBiller(It.IsAny<SavedBiller>()))
            .Returns((SavedBiller savedBiller) => savedBiller);

        // Act
        ErrorOr<SavedBillerDataTransferObject> result = _service.SaveBiller(UserId, request);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.BillerName.Should().Be("Internet Co");
        result.Value.Nickname.Should().Be("Home Internet");
    }

    /// <summary>
    ///     Verifies that SaveBiller returns BillerNotFound when the biller does not exist.
    /// </summary>
    [Fact]
    public void SaveBiller_WhenBillerNotFound_ReturnsBillerNotFoundError()
    {
        // Arrange
        var request = new SaveBillerRequest { BillerId = BillerId };
        _billerRepository.Setup(repository => repository.GetBillerById(BillerId)).Returns(BillerErrors.BillerNotFound);

        // Act
        ErrorOr<SavedBillerDataTransferObject> result = _service.SaveBiller(UserId, request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BillerErrors.BillerNotFound);
    }

    /// <summary>
    ///     Verifies that SaveBiller returns BillerAlreadySaved when the biller is already in the user's saved list.
    /// </summary>
    [Fact]
    public void SaveBiller_WhenAlreadySaved_ReturnsBillerAlreadySavedError()
    {
        // Arrange
        var biller = new Biller { Id = BillerId, Name = "Water Co", Category = "Utilities" };
        var existing = new List<SavedBiller>
        {
            new() { Id = SavedBillerId, UserId = UserId, BillerId = BillerId, Biller = biller },
        };
        var request = new SaveBillerRequest { BillerId = BillerId };
        _billerRepository.Setup(repository => repository.GetBillerById(BillerId)).Returns(biller);
        _billerRepository.Setup(repository => repository.GetSavedBillers(UserId)).Returns(existing);

        // Act
        ErrorOr<SavedBillerDataTransferObject> result = _service.SaveBiller(UserId, request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BillerErrors.BillerAlreadySaved);
    }

    // RemoveSavedBiller tests.

    /// <summary>
    ///     Verifies that RemoveSavedBiller returns Success when the entry exists and belongs to the user.
    /// </summary>
    [Fact]
    public void RemoveSavedBiller_WhenEntryExists_ReturnsSuccess()
    {
        // Arrange
        var saved = new List<SavedBiller>
        {
            new() { Id = SavedBillerId, UserId = UserId, BillerId = BillerId },
        };
        _billerRepository.Setup(repository => repository.GetSavedBillers(UserId)).Returns(saved);
        _billerRepository.Setup(repository => repository.DeleteSavedBiller(SavedBillerId)).Returns(Result.Success);

        // Act
        ErrorOr<Success> result = _service.RemoveSavedBiller(UserId, SavedBillerId);

        // Assert
        result.IsError.Should().BeFalse();
    }

    /// <summary>
    ///     Verifies that RemoveSavedBiller returns SavedBillerNotFound when the entry is not in the user's list.
    /// </summary>
    [Fact]
    public void RemoveSavedBiller_WhenEntryNotFound_ReturnsSavedBillerNotFoundError()
    {
        // Arrange
        _billerRepository.Setup(repository => repository.GetSavedBillers(UserId)).Returns(new List<SavedBiller>());

        // Act
        ErrorOr<Success> result = _service.RemoveSavedBiller(UserId, SavedBillerId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(BillerErrors.SavedBillerNotFound);
    }
}
