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
            // Hardcoded user credentials and roles for testing purposes
            var users = new Dictionary<string, (string Password, string Role)>
            {
                { "Andi", ("admin123", "admin") },
                { "Usuf", ("user123", "user") },
                { "Mani", ("manager123", "manager") }
            };

            if (!users.TryGetValue(loginRequest.Username, out var userInfo) || userInfo.Password != loginRequest.Password)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var userRole = userInfo.Role;

            // Generate JWT access token with role claim
            var accessToken = await _authService.GenerateToken(loginRequest.Username, userRole);

            // Generate refresh token
            var refreshToken = await _authService.GenerateRefreshToken();

            // Store refresh token
            await _authService.StoreRefreshToken(loginRequest.Username, refreshToken);

            // Set cookies for access token and refresh token
            Response.Cookies.Append("auth_token", accessToken, new CookieOptions
            {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = DateTime.UtcNow.AddMinutes(2)
            });

            Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
            {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new { message = "Login successful", role = userRole });
        }
        





        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new { message = "Refresh token is missing" });
            }

            // Forward the refresh token to SRMAuth for invalidation
            await _authService.InvalidateRefreshToken(refreshToken);

            // Clear cookies
            Response.Cookies.Delete("auth_token");
            Response.Cookies.Delete("refresh_token");

            return Ok(new { message = "Logged out successfully" });
        }


        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var oldRefreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(oldRefreshToken)) return Unauthorized(new { message = "Refresh token is missing" });

            var username = await _authService.ValidateRefreshToken(oldRefreshToken);
            if (username == null) return Unauthorized(new { message = "Invalid refresh token" });

            var newAccessToken = await _authService.GenerateToken(username, "admin");
            var newRefreshToken = Guid.NewGuid().ToString();

            await _authService.StoreRefreshToken(username, newRefreshToken);
            await _authService.InvalidateRefreshToken(oldRefreshToken);

            Response.Cookies.Append("auth_token", newAccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTime.UtcNow.AddMinutes(15)
            });

            Response.Cookies.Append("refresh_token", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new { message = "Token refreshed successfully" });
        }
        

        [HttpGet("role")]
        public async Task<IActionResult> GetUserRole()
        {
            var token = Request.Cookies["auth_token"];
            Console.WriteLine(token); 
            if (string.IsNullOrEmpty(token)) return Unauthorized();

            // Forward the token to SRMAuth for role extraction
            var role = await _authService.GetUserRoleFromToken(token);
            Console.WriteLine(role);
            if (role == null) return Unauthorized();

            return Ok(new { role });
        }

        [HttpGet("validate")]
        public async Task<IActionResult> ValidateAuthentication()
        {
            var token = Request.Cookies["auth_token"];
            if (string.IsNullOrEmpty(token)) return Unauthorized();

            // Forward the token to SRMAuth for validation
            var isValid = await _authService.ValidateToken(token);
            return Ok(new { isAuthenticated = isValid });
        }
    }
    

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}