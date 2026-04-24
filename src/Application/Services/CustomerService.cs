using ERP.Application.Abstractions;
using ERP.Application.DTOs.Customers;
using ERP.Domain.Entities;

namespace ERP.Application.Services;

public class CustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICepLookupService _cepLookupService;

    public CustomerService(ICustomerRepository customerRepository, ICepLookupService cepLookupService)
    {
        _customerRepository = customerRepository;
        _cepLookupService = cepLookupService;
    }

    public async Task<List<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetAllAsync(cancellationToken);
        return customers.Select(Map).ToList();
    }

    public async Task<CustomerResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        return customer is null ? null : Map(customer);
    }

    public async Task<CustomerResponse> CreateAsync(CustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = new Customer();
        await ApplyRequestAsync(customer, request, cancellationToken);

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        return Map(customer);
    }

    public async Task<CustomerResponse?> UpdateAsync(Guid id, CustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customer is null)
        {
            return null;
        }

        await ApplyRequestAsync(customer, request, cancellationToken);
        customer.UpdatedAtUtc = DateTime.UtcNow;

        await _customerRepository.SaveChangesAsync(cancellationToken);
        return Map(customer);
    }

    private async Task ApplyRequestAsync(Customer customer, CustomerRequest request, CancellationToken cancellationToken)
    {
        customer.Name = request.Name;
        customer.Document = request.Document;
        customer.Email = request.Email;
        customer.Phone = request.Phone;
        customer.Cep = CleanCep(request.Cep);
        customer.Number = request.Number;

        if (!string.IsNullOrWhiteSpace(customer.Cep))
        {
            var address = await _cepLookupService.LookupAsync(customer.Cep, cancellationToken);
            if (address is not null)
            {
                customer.Street = string.IsNullOrWhiteSpace(request.Street) ? address.Street : request.Street;
                customer.Neighborhood = string.IsNullOrWhiteSpace(request.Neighborhood) ? address.Neighborhood : request.Neighborhood;
                customer.City = string.IsNullOrWhiteSpace(request.City) ? address.City : request.City;
                customer.State = string.IsNullOrWhiteSpace(request.State) ? address.State : request.State;
                return;
            }
        }

        customer.Street = request.Street;
        customer.Neighborhood = request.Neighborhood;
        customer.City = request.City;
        customer.State = request.State;
    }

    private static string CleanCep(string cep) => new(cep.Where(char.IsDigit).ToArray());

    private static CustomerResponse Map(Customer customer) =>
        new(
            customer.Id,
            customer.Name,
            customer.Document,
            customer.Email,
            customer.Phone,
            customer.Cep,
            customer.Street,
            customer.Number,
            customer.Neighborhood,
            customer.City,
            customer.State,
            customer.CreatedAtUtc);
}
