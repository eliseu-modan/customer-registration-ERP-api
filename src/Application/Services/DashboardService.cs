using ERP.Application.Abstractions;
using ERP.Application.DTOs.Dashboard;
using ERP.Domain.Enums;

namespace ERP.Application.Services;

public class DashboardService
{
    private readonly IOrderRepository _orderRepository;

    public DashboardService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<DashboardResponse> GetAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);

        var totalSales = orders
            .Where(order => order.Status == OrderStatus.Paid)
            .Sum(order => order.TotalAmount);

        var byStatus = orders
            .GroupBy(order => order.Status.ToString())
            .Select(group => new OrderStatusSummary(group.Key, group.Count()))
            .OrderBy(summary => summary.Status)
            .ToList();

        return new DashboardResponse(totalSales, orders.Count, byStatus);
    }
}
