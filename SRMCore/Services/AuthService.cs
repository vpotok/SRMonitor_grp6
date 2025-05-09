using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SRMCore.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

       public async Task<string> GenerateToken(string username, string role)
        {
            var response = await _httpClient.PostAsJsonAsync("http://srmauth:80/api/auth/generate-token", new
            {
                username,
                role
            });

            response.EnsureSuccessStatusCode();

            // Deserialize the response to extract the token
            var result = await response.Content.ReadFromJsonAsync<GenerateTokenResponse>();
            return result?.Token ?? throw new InvalidOperationException("Token generation failed");
        }

        public async Task<bool> ValidateToken(string token)
        {
            var response = await _httpClient.PostAsJsonAsync("http://srmauth:80/api/auth/validate-token", new { token });
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<TokenValidationResponse>();
            return result?.IsValid ?? false;
        }

        public async Task<string?> GetUserFromToken(string token)
        {
            var response = await _httpClient.PostAsJsonAsync("http://srmauth:80/api/auth/validate-token", new { token });
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<TokenValidationResponse>();
            return result?.Username;
        }

        public class TokenValidationResponse
        {
            public bool IsValid { get; set; }
            public string? Username { get; set; }
        }
             // Helper class for deserializing the response
        private class GenerateTokenResponse
        {
            public string Token { get; set; } = string.Empty;
        }
    }
}