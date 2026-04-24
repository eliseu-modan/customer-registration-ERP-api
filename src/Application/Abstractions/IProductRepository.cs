using ERP.Domain.Entities;

namespace ERP.Application.Abstractions;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Product>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}
