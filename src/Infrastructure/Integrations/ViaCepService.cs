using System.Net.Http.Json;
using ERP.Application.Abstractions;
using ERP.Application.DTOs.Shared;

namespace ERP.Infrastructure.Integrations;

public class ViaCepService : ICepLookupService
{
    private readonly HttpClient _httpClient;

    public ViaCepService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AddressLookupResult?> LookupAsync(string cep, CancellationToken cancellationToken = default)
    {
        var normalizedCep = new string(cep.Where(char.IsDigit).ToArray());
        if (normalizedCep.Length != 8)
        {
            return null;
        }

        try
        {
            var response = await _httpClient.GetFromJsonAsync<ViaCepResponse>($"{normalizedCep}/json/", cancellationToken);
            if (response is null || response.Erro)
            {
                return null;
            }

            return new AddressLookupResult(
                response.Cep ?? normalizedCep,
                response.Logradouro ?? string.Empty,
                response.Bairro ?? string.Empty,
                response.Localidade ?? string.Empty,
                response.Uf ?? string.Empty);
        }
        catch
        {
            return null;
        }
    }

    private sealed class ViaCepResponse
    {
        public string? Cep { get; set; }
        public string? Logradouro { get; set; }
        public string? Bairro { get; set; }
        public string? Localidade { get; set; }
        public string? Uf { get; set; }
        public bool Erro { get; set; }
    }
}
