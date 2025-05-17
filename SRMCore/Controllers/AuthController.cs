using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SRMCore.Models;
using SRMCore.Services;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json;

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
        if (user == null) return Unauthorized("Ungültige Zugangsdaten.");

        Console.WriteLine($"🧪 TOKEN-REQUEST: uid={user.UserId}, role={user.Role}, comId={user.ComId}");

        var token = await _tokenService.RequestTokenAsync(user.UserId, user.Role, user.ComId);

        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        });

        return Ok(new { success = true });
    }

    [HttpGet("role")]
    public async Task<IActionResult> GetRole([FromServices] IConfiguration config, [FromServices] IHttpClientFactory httpClientFactory)
    {
        // 1. Token extrahieren
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
        {
            token = Request.Cookies["jwt"];
            //Console.WriteLine($"Cookie-Token: {token}");
        }

        if (string.IsNullOrEmpty(token))
            return Unauthorized("Kein Token vorhanden.");

        // 2. Anfrage an Token-Service vorbereiten
        var httpClient = httpClientFactory.CreateClient();
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{config["TokenService:BaseUrl"]}/api/validate");
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        httpRequest.Content = new StringContent("{}", Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.SendAsync(httpRequest);

            Console.WriteLine($"📥 TOKEN-VALIDIERUNG: StatusCode = {response.StatusCode}");
            var rawResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"📦 TOKEN-RESPONSE: {rawResponse}");

            if (!response.IsSuccessStatusCode)
            {
                return Unauthorized("Token ungültig.");
            }

            var result = JsonConvert.DeserializeObject<TokenValidationResponse>(rawResponse);

            return Ok(new
            {
                role = result?.Role
            });

        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fehler beim Token-Service: {ex.Message}");
            return StatusCode(500, "Fehler bei der Tokenvalidierung.");
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromServices] IConfiguration _config,
        [FromServices] IHttpClientFactory _httpClientFactory)
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
        {
            token = Request.Cookies["jwt"];
        }

        if (string.IsNullOrEmpty(token))
            return Unauthorized("Kein Token vorhanden.");

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_config["TokenService:BaseUrl"]}/api/logout");
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            httpRequest.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(httpRequest);
            Console.WriteLine($"🔐 LOGOUT-Request an TokenService: {response.StatusCode}");

            Response.Cookies.Delete("jwt");

            return Ok(new { message = "Erfolgreich abgemeldet" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fehler beim Logout-Call an TokenService: {ex.Message}");
            return StatusCode(500, "Fehler beim Abmelden.");
        }
    }

}