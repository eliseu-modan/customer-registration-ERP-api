namespace ERP.Application.DTOs.Customers;

public record CustomerResponse(
    Guid Id,
    string Name,
    string Document,
    string Email,
    string Phone,
    string Cep,
    string Street,
    string Number,
    string Neighborhood,
    string City,
    string State,
    DateTime CreatedAtUtc);
