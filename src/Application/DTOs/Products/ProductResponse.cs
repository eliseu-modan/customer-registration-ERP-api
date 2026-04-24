namespace ERP.Application.DTOs.Products;

public record ProductResponse(Guid Id, string Name, string Sku, decimal Price);
