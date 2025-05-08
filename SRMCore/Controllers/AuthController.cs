using Microsoft.AspNetCore.Mvc;
using SRMCore.Models;
using SRMCore.Services;

namespace SRMCore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest loginRequest)
    {
        var user = _authService.Authenticate(loginRequest.Username, loginRequest.Password);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        return Ok(new { message = "Login successful", user });
    }
}