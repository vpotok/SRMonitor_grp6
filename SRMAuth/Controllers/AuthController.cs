using Microsoft.AspNetCore.Mvc;
using SRMAuth.Models;
using SRMAuth.Services;

namespace SRMAuth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TokenService _tokenService;

    public AuthController(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request.Username == "admin" && request.Password == "secret")
        {
            var token = _tokenService.GenerateToken(request.Username, isAdmin: true);
            return Ok(new { token });
        }

        if (request.Username == "kunde" && request.Password == "1234")
        {
            var token = _tokenService.GenerateToken(request.Username, isAdmin: false);
            return Ok(new { token });
        }

        return Unauthorized();
    }
}
