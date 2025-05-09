using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace SRMCore.Services;

public class RedmineService : IRedmineService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public RedmineService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task CreateTicketAsync(string subject, string description)
    {
        var baseUrl = _config["Redmine:BaseUrl"];
        var apiKey = _config["Redmine:ApiKey"];

        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/issues.json");
        request.Headers.Add("X-Redmine-API-Key", apiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var body = new
        {
            issue = new
            {
                project_id = 1,
                tracker_id = 1,
                priority_id = 1,
                status_id = 1,
                subject = subject,
                description = description
            }
        };

        var json = JsonSerializer.Serialize(body);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
