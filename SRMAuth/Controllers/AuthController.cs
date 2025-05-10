using Microsoft.AspNetCore.Mvc;
using SRMAuth.Services;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

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

    [HttpPost("validate-token")]
    public IActionResult ValidateToken([FromBody] TokenValidationRequest request)
    {
        if (string.IsNullOrEmpty(request.Token))
        {
            return BadRequest(new { message = "Token is required" });
        }

        var principal = _tokenService.ValidateToken(request.Token);
        if (principal == null)
        {
            return Ok(new { isValid = false });
        }

        var username = principal.Identity?.Name; // Extract the username (subject)
        return Ok(new { isValid = true, username });
    }

    [HttpPost("get-role")]
    public IActionResult GetRole([FromBody] TokenValidationRequest request)
    {
        if (string.IsNullOrEmpty(request.Token))
        {
            return BadRequest(new { message = "Token is required" });
        }

        var principal = _tokenService.ValidateToken(request.Token);
        if (principal == null)
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        // Use ClaimTypes.Role to extract the role claim
        var role = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        return Ok(new { role });
    }
    
    [HttpPost("generate-refresh-token")]
    public IActionResult GenerateRefreshToken()
    {
        var refreshToken = _tokenService.GenerateRefreshToken();
        return Ok(new { refreshToken });
    }

    [HttpPost("store-refresh-token")]
    public async Task<IActionResult> StoreRefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest(new { message = "Invalid request" });
        }

        try
        {
            await _tokenService.StoreRefreshToken(request.Username, request.RefreshToken);
            return Ok(new { message = "Refresh token stored successfully" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error storing refresh token: {ex.Message}");
            return StatusCode(500, new { message = "Failed to store refresh token" });
        }
    }


   [HttpPost("validate-refresh-token")]
    public async Task<IActionResult> ValidateRefreshToken([FromBody] RefreshTokenValidationRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest(new { message = "Refresh token is required" });
        }

        try
        {
            var username = await _tokenService.ValidateRefreshToken(request.RefreshToken);
            if (username == null)
            {
                return NotFound(new { message = "Refresh token not found or invalid" });
            }

            return Ok(new { username });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error validating refresh token: {ex.Message}");
            return StatusCode(500, new { message = "Failed to validate refresh token" });
        }
    }

    [HttpPost("invalidate-refresh-token")]
    public async Task<IActionResult> InvalidateRefreshToken([FromBody] InvalidateRefreshTokenUser request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest(new { message = "Refresh token is required" });
        }

        try
        {
            await _tokenService.InvalidateRefreshToken(request.RefreshToken);
            return Ok(new { message = "Refresh token invalidated successfully" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error invalidating refresh token: {ex.Message}");
            return StatusCode(500, new { message = "Failed to invalidate refresh token" });
        }
    }

    [HttpPost("rotate-refresh-token")]
    public async Task<IActionResult> RotateRefreshToken([FromBody] RefreshTokenRotationRequest request)
    {
        if (string.IsNullOrEmpty(request.OldRefreshToken))
        {
            return BadRequest(new { message = "Old refresh token is required" });
        }

        try
        {
            var username = await _tokenService.ValidateRefreshToken(request.OldRefreshToken);
            if (username == null)
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }

            var newRefreshToken = Guid.NewGuid().ToString();
            await _tokenService.StoreRefreshToken(username, newRefreshToken);
            await _tokenService.InvalidateRefreshToken(request.OldRefreshToken);

            return Ok(new { newRefreshToken });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error rotating refresh token: {ex.Message}");
            return StatusCode(500, new { message = "Failed to rotate refresh token" });
        }
    }

    public class InvalidateRefreshTokenUser
    {
        public string RefreshToken { get; set; } = string.Empty;
    }


    public class RefreshTokenRotationRequest
    {
        public string OldRefreshToken { get; set; } = string.Empty;
    }

    public class RefreshTokenValidationRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshTokenRequest
    {
        public string Username { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

}
public class TokenValidationRequest
{
    public string Token { get; set; } = string.Empty;
}


public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}