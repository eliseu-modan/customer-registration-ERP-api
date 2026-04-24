using ERP.Application.Abstractions;
using ERP.Domain.Entities;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(Customer customer, CancellationToken cancellationToken = default) =>
        _context.Customers.AddAsync(customer, cancellationToken).AsTask();

    public Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.Customers.OrderBy(customer => customer.Name).ToListAsync(cancellationToken);

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Customers.FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
