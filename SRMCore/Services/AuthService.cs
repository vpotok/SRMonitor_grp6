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
            return await response.Content.ReadAsStringAsync();
        }
    }
}