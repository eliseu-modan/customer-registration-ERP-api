using ERP.Application.DTOs.Shared;

namespace ERP.Application.Abstractions;

public interface ICepLookupService
{
    Task<AddressLookupResult?> LookupAsync(string cep, CancellationToken cancellationToken = default);
}
