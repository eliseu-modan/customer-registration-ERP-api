using ERP.Application.Abstractions;
using ERP.Domain.Entities;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(Order order, CancellationToken cancellationToken = default) =>
        _context.Orders.AddAsync(order, cancellationToken).AsTask();

    public Task<List<Order>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.Orders
            .Include(order => order.Customer)
            .Include(order => order.Items)
            .ThenInclude(item => item.Product)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Orders
            .Include(order => order.Customer)
            .Include(order => order.Items)
            .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
