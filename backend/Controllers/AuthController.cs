using Microsoft.AspNetCore.Mvc;
using SentinelWealth.Api.Application.Abstractions;

namespace SentinelWealth.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IPortfolioRepository _repository;

    public AuthController(IPortfolioRepository repository)
    {
        _repository = repository;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "usuario y contraseña requeridos." });
        }

        var user = _repository.FindUser(request.Username.Trim(), request.Password);
        if (user is null)
        {
            return Unauthorized(new { error = "credenciales inválidas." });
        }

        return Ok(new
        {
            user.Id,
            user.Username,
            user.ClientName,
            user.Tier
        });
    }

    public sealed class LoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
