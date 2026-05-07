namespace BankingApp.Application.Features.Billers.Services;

using BankingApp.Application.Features.Billers.Dtos;
using ErrorOr;

/// <summary>
///     Defines use-case operations for the biller directory and saved-biller management.
/// </summary>
public interface IBillerService
{
    /// <summary>Returns all active billers.</summary>
    /// <returns>The list of active billers.</returns>
    public ErrorOr<List<BillerDto>> GetBillerDirectory();

    /// <summary>Searches the biller directory by name and/or category.</summary>
    /// <param name="searchTerm">Text to match against biller names.</param>
    /// <param name="category">Optional category filter.</param>
    /// <returns>The matching billers.</returns>
    public ErrorOr<List<BillerDto>> SearchBillers(string searchTerm, string? category = null);

    /// <summary>Returns all billers saved by the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The list of saved biller DTOs.</returns>
    public ErrorOr<List<SavedBillerDto>> GetSavedBillers(int userId);

    /// <summary>Saves a biller for the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="request">The save request data.</param>
    /// <returns>The created saved biller DTO, or an error.</returns>
    public ErrorOr<SavedBillerDto> SaveBiller(int userId, SaveBillerRequest request);

    /// <summary>Removes a saved biller for the specified user.</summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="savedBillerId">The saved biller entry identifier.</param>
    /// <returns>Success or an error.</returns>
    public ErrorOr<Success> RemoveSavedBiller(int userId, int savedBillerId);
}
