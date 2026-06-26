using Microsoft.AspNetCore.Mvc;
using Restaurant.Api.Dtos;
using Restaurant.Api.Services;

namespace Restaurant.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await authService.LoginAsync(request);
        if (user is null)
        {
            return Unauthorized(new { message = "Usuario o contrasena incorrectos." });
        }

        return Ok(user);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(LoginRequest request)
    {
        var (user, error) = await authService.RegisterAsync(request);
        if (user is null)
        {
            return BadRequest(new { message = error ?? "No se pudo crear el usuario." });
        }

        return CreatedAtAction(nameof(Register), user);
    }
}
