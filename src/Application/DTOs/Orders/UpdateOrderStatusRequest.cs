using System.ComponentModel.DataAnnotations;

namespace ERP.Application.DTOs.Orders;

public class UpdateOrderStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
