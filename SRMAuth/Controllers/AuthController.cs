using Microsoft.AspNetCore.Mvc;
using SRMAuth.Services;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
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

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("0101YourSuperSecretKeyHereThatIsAtLeast32CharsLong0101");

        try
        {
            tokenHandler.ValidateToken(request.Token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = "SRMAuth",
                ValidAudience = "SRMCore",
                ValidateLifetime = true
            }, out SecurityToken validatedToken);

            // Extract user information (e.g., username) from the token
            var jwtToken = (JwtSecurityToken)validatedToken;
            var username = jwtToken.Subject;

            return Ok(new { isValid = true, username });
        }
        catch
        {
            return Ok(new { isValid = false });
        }
    }

public class TokenValidationRequest
{
    public string Token { get; set; } = string.Empty;
}
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}