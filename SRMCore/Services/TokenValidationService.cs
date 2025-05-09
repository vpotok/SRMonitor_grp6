using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using SRMCore.Models;

namespace SRMCore.Services;

public class TokenValidationService : ITokenValidationService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public TokenValidationService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<TokenValidationResponse?> ValidateTokenAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_config["TokenService:BaseUrl"]}/api/validate");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<TokenValidationResponse>();
    }
}
