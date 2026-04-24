using ERP.Application.Abstractions;
using ERP.Application.DTOs.Products;

namespace ERP.Application.Services;

public class ProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllActiveAsync(cancellationToken);
        return products.Select(product => new ProductResponse(product.Id, product.Name, product.Sku, product.Price)).ToList();
    }
}
