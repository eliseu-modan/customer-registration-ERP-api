using ERP.Application.Abstractions;
using ERP.Application.DTOs.Orders;
using ERP.Domain.Entities;
using ERP.Domain.Enums;

namespace ERP.Application.Services;

public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;

    public OrderService(IOrderRepository orderRepository, ICustomerRepository customerRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
    }

    public async Task<List<OrderResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        return orders.Select(Map).ToList();
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : Map(order);
    }

    public async Task<OrderResponse?> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
        {
            return null;
        }

        var order = new Order
        {
            CustomerId = customer.Id,
            Customer = customer,
            Status = OrderStatus.Pending
        };

        foreach (var itemRequest in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemRequest.ProductId, cancellationToken);
            if (product is null || !product.Active)
            {
                throw new InvalidOperationException($"Produto {itemRequest.ProductId} não encontrado.");
            }

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Product = product,
                Quantity = itemRequest.Quantity,
                UnitPrice = product.Price,
                TotalPrice = product.Price * itemRequest.Quantity
            });
        }

        order.TotalAmount = order.Items.Sum(item => item.TotalPrice);

        await _orderRepository.AddAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        return Map(order);
    }

    public async Task<OrderResponse?> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return null;
        }

        if (!Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
        {
            throw new InvalidOperationException("Status inválido. Use Pending, Paid ou Cancelled.");
        }

        order.Status = parsedStatus;
        order.UpdatedAtUtc = DateTime.UtcNow;

        await _orderRepository.SaveChangesAsync(cancellationToken);
        return Map(order);
    }

    private static OrderResponse Map(Order order) =>
        new(
            order.Id,
            order.CustomerId,
            order.Customer?.Name ?? string.Empty,
            order.Status.ToString(),
            order.TotalAmount,
            order.CreatedAtUtc,
            order.Items.Select(item => new OrderItemResponse(
                item.ProductId,
                item.Product?.Name ?? string.Empty,
                item.Quantity,
                item.UnitPrice,
                item.TotalPrice)).ToList());
}
