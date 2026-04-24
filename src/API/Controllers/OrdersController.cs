using ERP.Application.DTOs.Orders;
using ERP.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromServices] OrderService orderService, CancellationToken cancellationToken) =>
        Ok(await orderService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, [FromServices] OrderService orderService, CancellationToken cancellationToken)
    {
        var order = await orderService.GetByIdAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromServices] OrderService orderService, [FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderService.CreateAsync(request, cancellationToken);
            return order is null
                ? BadRequest(new { message = "Cliente não encontrado." })
                : CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromServices] OrderService orderService, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderService.UpdateStatusAsync(id, request.Status, cancellationToken);
            return order is null ? NotFound() : Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
