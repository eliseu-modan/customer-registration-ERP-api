namespace ERP.Application.DTOs.Dashboard;

public record DashboardResponse(decimal TotalSales, int TotalOrders, IReadOnlyCollection<OrderStatusSummary> OrdersByStatus);

public record OrderStatusSummary(string Status, int Count);
