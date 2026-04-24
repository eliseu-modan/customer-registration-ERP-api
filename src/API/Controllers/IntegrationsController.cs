using ERP.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IntegrationsController : ControllerBase
{
    [HttpGet("cep/{cep}")]
    public async Task<IActionResult> LookupCep(string cep, [FromServices] ICepLookupService cepLookupService, CancellationToken cancellationToken)
    {
        var address = await cepLookupService.LookupAsync(cep, cancellationToken);
        return address is null ? NotFound(new { message = "CEP não encontrado." }) : Ok(address);
    }
}
