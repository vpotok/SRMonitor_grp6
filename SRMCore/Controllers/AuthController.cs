using Microsoft.AspNetCore.Mvc;
using SRMCore.Models;
using SRMCore.Services;

namespace SRMCore.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;

    public AuthController(IUserService userService, ITokenService tokenService)
    {
        _userService = userService;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userService.AuthenticateAsync(request.Username, request.Password);
        if (user == null)
            return Unauthorized("Ungültiger Benutzername oder Passwort.");

        var token = await _tokenService.RequestTokenAsync(user.Id, user.Role, user.CustomerId);
        return Ok(new { token });
    }
}
