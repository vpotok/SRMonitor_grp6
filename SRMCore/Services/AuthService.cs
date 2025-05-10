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

            var result = await response.Content.ReadFromJsonAsync<GenerateTokenResponse>();
            return result?.Token ?? throw new InvalidOperationException("Token generation failed");
        }

        public async Task<string?> GetUserRoleFromToken(string token)
        {
            var response = await _httpClient.PostAsJsonAsync("http://srmauth:80/api/auth/get-role", new { token });
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<GetRoleResponse>();
            return result?.Role;
        }

        public async Task<bool> ValidateToken(string token)
        {
            var response = await _httpClient.PostAsJsonAsync("http://srmauth:80/api/auth/validate-token", new { token });
            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<ValidateTokenResponse>();
            return result?.IsValid ?? false;
        }
        public async Task StoreRefreshToken(string username, string refreshToken)
        {
            var response = await _httpClient.PostAsJsonAsync("http://srmauth:80/api/auth/store-refresh-token", new { username, refreshToken });
            response.EnsureSuccessStatusCode();
        }

        public async Task<string?> ValidateRefreshToken(string refreshToken)
        {
            var response = await _httpClient.PostAsJsonAsync("http://srmauth:80/api/auth/validate-refresh-token", new { refreshToken });
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<ValidateRefreshTokenResponse>();
            return result?.Username;
        }

        public async Task InvalidateRefreshToken(string refreshToken)
        {
            var response = await _httpClient.PostAsJsonAsync("http://srmauth:80/api/auth/invalidate-refresh-token", new { refreshToken });
            response.EnsureSuccessStatusCode();
        }


        public async Task<(string NewAccessToken, string NewRefreshToken)?> RotateRefreshToken(string oldRefreshToken)
        {
            var response = await _httpClient.PostAsJsonAsync("http://srmauth:80/api/auth/rotate-refresh-token", new { OldRefreshToken = oldRefreshToken });

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<RotateRefreshTokenResponse>();
            return (result?.NewAccessToken ?? string.Empty, result?.NewRefreshToken ?? string.Empty);
        }
        public async Task<string> GenerateRefreshToken()
        {
            var response = await _httpClient.PostAsync("http://srmauth:80/api/auth/generate-refresh-token", null);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GenerateRefreshTokenResponse>();
            return result?.RefreshToken ?? throw new InvalidOperationException("Failed to generate refresh token");
        }
    }
    
    public class InvalidateRefreshTokenResponse
    {
        public string Message { get; set; } = string.Empty;
    }

    public class GenerateRefreshTokenResponse
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
    
    
    public class RotateRefreshTokenResponse
    {
        public string NewAccessToken { get; set; } = string.Empty;
        public string NewRefreshToken { get; set; } = string.Empty;
    }

    public class ValidateRefreshTokenResponse
    {
        public string Username { get; set; } = string.Empty;
    }
        


    public class GenerateTokenResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    public class GetRoleResponse
    {
        public string Role { get; set; } = string.Empty;
    }

    public class ValidateTokenResponse
    {
        public bool IsValid { get; set; }
    }
}
