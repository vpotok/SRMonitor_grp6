using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using SRMCore.Models;

namespace SRMCore.Services;

public class TokenService : ITokenService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public TokenService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<string> RequestTokenAsync(int userId, string role, int customerId)
    {
        var request = new TokenRequest
        {
            UserId = userId,
            Role = role,
            CustomerId = customerId
        };

        Console.WriteLine($"➡️ Sending TokenRequest: userId={userId}, role={role}, customerId={customerId}");

        var response = await _http.PostAsJsonAsync(
            $"{_config["TokenService:BaseUrl"]}/api/token", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ Token request failed: {response.StatusCode} – {error}");
            return string.Empty;
        }

        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        return result?["token"] ?? string.Empty;
    }
}
