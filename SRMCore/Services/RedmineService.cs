using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SRMCore.Data;

namespace SRMCore.Services;

public class RedmineService
{
    private readonly CoreDbContext _db;
    private readonly HttpClient _http;

    public RedmineService(CoreDbContext db, HttpClient http)
    {
        _db = db;
        _http = http;
    }

    public async Task CreateTicketAsync(int comId, string subject, string description)
    {
        var redmine = await _db.Redmines.FirstOrDefaultAsync(r => r.ComId == comId);
        if (redmine == null) return;

        var req = new HttpRequestMessage(HttpMethod.Post, "https://redmine:443/issues.json");
        req.Headers.Add("X-Redmine-API-Key", redmine.ApiKey);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var body = new
        {
            issue = new
            {
                project_id = 1,
                tracker_id = 1,
                priority_id = 1,
                status_id = 1,
                subject,
                description
            }
        };

        var json = JsonSerializer.Serialize(body);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(req);
        response.EnsureSuccessStatusCode();
    }
}
