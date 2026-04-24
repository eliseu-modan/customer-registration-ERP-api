using ERP.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromServices] ProductService productService, CancellationToken cancellationToken) =>
        Ok(await productService.GetAllAsync(cancellationToken));
}
