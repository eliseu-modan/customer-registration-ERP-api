using System.ComponentModel.DataAnnotations;

namespace ERP.Application.DTOs.Customers;

public class CustomerRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Document { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
