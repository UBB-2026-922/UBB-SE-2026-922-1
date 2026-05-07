namespace BankingApp.Application.Features.Billers.Services;

using BankingApp.Application.Features.Billers.Repositories;
using BankingApp.Application.Features.Billers.Dtos;

using Domain.Entities;
using Domain.Errors;
using ErrorOr;

/// <summary>
///     Provides biller directory and saved-biller management use cases.
/// </summary>
public class BillerService : IBillerService
{
    private readonly IBillerRepository _billerRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BillerService" /> class.
    /// </summary>
    /// <param name="billerRepository">The biller repository.</param>
    public BillerService(IBillerRepository billerRepository)
    {
        _billerRepository = billerRepository;
    }

    /// <inheritdoc />
    public ErrorOr<List<BillerDto>> GetBillerDirectory()
    {
        ErrorOr<List<Biller>> result = _billerRepository.GetAllBillers();
        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.ConvertAll(ToDto);
    }

    /// <inheritdoc />
    public ErrorOr<List<BillerDto>> SearchBillers(string searchTerm, string? category = null)
    {
        ErrorOr<List<Biller>> result = _billerRepository.SearchBillers(searchTerm, category);
        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.ConvertAll(ToDto);
    }

    /// <inheritdoc />
    public ErrorOr<List<SavedBillerDto>> GetSavedBillers(int userId)
    {
        ErrorOr<List<SavedBiller>> result = _billerRepository.GetSavedBillers(userId);
        if (result.IsError)
        {
            return result.Errors;
        }

        return result.Value.ConvertAll(ToSavedDto);
    }

    /// <inheritdoc />
    public ErrorOr<SavedBillerDto> SaveBiller(int userId, SaveBillerRequest request)
    {
        ErrorOr<Biller> billerResult = _billerRepository.GetBillerById(request.BillerId);
        if (billerResult.IsError)
        {
            return BillerErrors.BillerNotFound;
        }

        ErrorOr<List<SavedBiller>> existingResult = _billerRepository.GetSavedBillers(userId);
        if (!existingResult.IsError &&
            existingResult.Value.Any(savedBiller => savedBiller.BillerId == request.BillerId))
        {
            return BillerErrors.BillerAlreadySaved;
        }

        var savedBiller = new SavedBiller
        {
            UserId = userId,
            BillerId = request.BillerId,
            Nickname = request.Nickname,
            DefaultReference = request.DefaultReference,
            CreatedAt = DateTime.UtcNow
        };

        ErrorOr<SavedBiller> saveResult = _billerRepository.SaveBiller(savedBiller);
        if (saveResult.IsError)
        {
            return saveResult.Errors;
        }

        saveResult.Value.Biller = billerResult.Value;
        return ToSavedDto(saveResult.Value);
    }

    /// <inheritdoc />
    public ErrorOr<Success> RemoveSavedBiller(int userId, int savedBillerId)
    {
        ErrorOr<List<SavedBiller>> savedResult = _billerRepository.GetSavedBillers(userId);
        if (savedResult.IsError)
        {
            return savedResult.Errors;
        }

        SavedBiller? entry = savedResult.Value.FirstOrDefault(savedBiller => savedBiller.Id == savedBillerId);
        if (entry is null)
        {
            return BillerErrors.SavedBillerNotFound;
        }

        return _billerRepository.DeleteSavedBiller(savedBillerId);
    }

    private static BillerDto ToDto(Biller biller)
    {
        return new BillerDto
        {
            Id = biller.Id,
            Name = biller.Name,
            Category = biller.Category,
            LogoUrl = biller.LogoUrl,
            IsActive = biller.IsActive
        };
    }

    private static SavedBillerDto ToSavedDto(SavedBiller savedBiller)
    {
        return new SavedBillerDto
        {
            Id = savedBiller.Id,
            UserId = savedBiller.UserId,
            BillerId = savedBiller.BillerId,
            BillerName = savedBiller.Biller?.Name ?? string.Empty,
            BillerCategory = savedBiller.Biller?.Category ?? string.Empty,
            LogoUrl = savedBiller.Biller?.LogoUrl,
            Nickname = savedBiller.Nickname,
            DefaultReference = savedBiller.DefaultReference,
            CreatedAt = savedBiller.CreatedAt,
            Biller = savedBiller.Biller is null ? null : ToDto(savedBiller.Biller)
        };
    }
}
