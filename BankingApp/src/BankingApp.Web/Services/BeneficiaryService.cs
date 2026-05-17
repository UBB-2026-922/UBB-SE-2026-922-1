namespace BankingApp.Web.Services;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BankingApp.Web.ViewModels;

public sealed class BeneficiaryService : IBeneficiaryService
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public BeneficiaryService(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<BeneficiaryListViewModel.BeneficiaryRow>> GetAllAsync(string token)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, "api/beneficiaries");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using HttpResponseMessage response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        string json = await response.Content.ReadAsStringAsync();
        List<BeneficiaryDto>? dtos = JsonSerializer.Deserialize<List<BeneficiaryDto>>(json, _jsonOptions);

        return dtos?.Select(d => new BeneficiaryListViewModel.BeneficiaryRow
        {
            Id = d.Id,
            Name = d.Name ?? string.Empty,
            Iban = d.Iban ?? string.Empty,
            BankName = d.BankName,
            TransferCount = d.TransferCount,
            TotalAmountSent = d.TotalAmountSent,
            LastTransferDate = d.LastTransferDate
        }).ToList() ?? [];
    }

    public async Task<EditBeneficiaryViewModel?> GetByIdAsync(int id, string token)
    {
        // The API has no single-get endpoint, so fetch all and filter.
        IReadOnlyList<BeneficiaryListViewModel.BeneficiaryRow> all = await GetAllAsync(token);
        BeneficiaryListViewModel.BeneficiaryRow? row = all.FirstOrDefault(b => b.Id == id);
        if (row is null)
        {
            return null;
        }

        return new EditBeneficiaryViewModel
        {
            Id = row.Id,
            Name = row.Name,
            Iban = row.Iban,
            BankName = row.BankName
        };
    }

    public async Task<bool> CreateAsync(CreateBeneficiaryViewModel model, string token)
    {
        string body = JsonSerializer.Serialize(new
        {
            name = model.Name,
            iban = model.Iban,
            bankName = model.BankName
        });

        using HttpRequestMessage request = new(HttpMethod.Post, "api/beneficiaries")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using HttpResponseMessage response = await _http.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(EditBeneficiaryViewModel model, string token)
    {
        string body = JsonSerializer.Serialize(new
        {
            name = model.Name,
            iban = model.Iban,
            bankName = model.BankName
        });

        using HttpRequestMessage request = new(HttpMethod.Put, $"api/beneficiaries/{model.Id}")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using HttpResponseMessage response = await _http.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id, string token)
    {
        using HttpRequestMessage request = new(HttpMethod.Delete, $"api/beneficiaries/{id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using HttpResponseMessage response = await _http.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    // Local DTO matching the API response shape.
    private sealed class BeneficiaryDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Iban { get; set; }
        public string? BankName { get; set; }
        public int TransferCount { get; set; }
        public decimal TotalAmountSent { get; set; }
        public DateTime? LastTransferDate { get; set; }
    }
}