using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

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
        var response = await _http.PostAsJsonAsync(
            $"{_config["TokenService:BaseUrl"]}/api/token",
            new { user_id = userId, role, customer_id = customerId });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        return result!["token"];
    }
}
