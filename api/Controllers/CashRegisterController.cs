using Microsoft.AspNetCore.Mvc;
using Restaurant.Api.Dtos;
using Restaurant.Api.Services;

namespace Restaurant.Api.Controllers;

[ApiController]
[Route("api/cash-register")]
public class CashRegisterController(ICashRegisterService cashRegister) : ControllerBase
{
    [HttpGet("current")]
    public async Task<ActionResult<CashRegisterDto?>> GetCurrent()
    {
        return Ok(await cashRegister.GetCurrentAsync());
    }

    [HttpGet("closed")]
    public async Task<ActionResult<IReadOnlyList<CashRegisterDto>>> GetClosed()
    {
        return Ok(await cashRegister.GetClosedAsync());
    }

    [HttpPost("open")]
    public async Task<ActionResult<CashRegisterDto>> Open(OpenCashRegisterRequest request)
    {
        try
        {
            return Ok(await cashRegister.OpenAsync(request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("close")]
    public async Task<ActionResult<CashRegisterDto>> Close()
    {
        var result = await cashRegister.CloseAsync();
        return result is null ? NotFound(new { message = "No hay una caja abierta." }) : Ok(result);
    }
}
