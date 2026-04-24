namespace ERP.Application.DTOs.Shared;

public record AddressLookupResult(
    string Cep,
    string Street,
    string Neighborhood,
    string City,
    string State);
