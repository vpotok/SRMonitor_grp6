using Microsoft.AspNetCore.Mvc;
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

    [HttpPost("generate-token")]
    public IActionResult GenerateToken([FromBody] LoginRequest loginRequest)
    {
        if (string.IsNullOrEmpty(loginRequest.Username) || string.IsNullOrEmpty(loginRequest.Role))
        {
            return BadRequest(new { message = "Username and Role are required" });
        }

        var token = _tokenService.GenerateToken(loginRequest.Username, loginRequest.Role);

        // Return the token as a JSON object
        return Ok(new { token });
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}