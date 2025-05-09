using Microsoft.AspNetCore.Mvc;
using SRMCore.Services;


namespace SRMCore.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            // Validate user credentials (replace with actual validation logic)
            if (loginRequest.Username != "admin" || loginRequest.Password != "admin123")
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Generate token using AuthService
            var token = await _authService.GenerateToken(loginRequest.Username, "admin");

            // Set the raw token as an HttpOnly cookie
            Response.Cookies.Append("auth_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Use HTTPS in production
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTime.UtcNow.AddHours(1)
            });

            return Ok(new { message = "Login successful" });
        }
        
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}