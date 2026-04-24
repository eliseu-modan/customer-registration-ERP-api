namespace ERP.Application.DTOs.Orders;

public record OrderResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAtUtc,
    IReadOnlyCollection<OrderItemResponse> Items);

public record OrderItemResponse(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal TotalPrice);
