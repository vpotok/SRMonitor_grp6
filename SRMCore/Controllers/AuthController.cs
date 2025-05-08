using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Npgsql;
using System.Data;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public AuthController(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand("SELECT * FROM users WHERE username = @u AND password = @p", conn);
        cmd.Parameters.AddWithValue("u", request.Username);
        cmd.Parameters.AddWithValue("p", request.Password); // Use hashing in production

        using var reader = await cmd.ExecuteReaderAsync();
        if (!reader.Read()) return Unauthorized();

        var client = _httpClientFactory.CreateClient();
        var tokenResponse = await client.PostAsync(
            _config["TokenService:Url"] + "/token",
            new StringContent(JsonSerializer.Serialize(new { username = request.Username }), Encoding.UTF8, "application/json")
        );

        if (!tokenResponse.IsSuccessStatusCode) return StatusCode(500);
        var jwt = await tokenResponse.Content.ReadAsStringAsync();

        Response.Cookies.Append("jwt", jwt, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Strict });
        return Ok(new { message = "Logged in" });
    }
}

public record LoginRequest(string Username, string Password);