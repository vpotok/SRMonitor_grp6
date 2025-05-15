using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SRMCore.Data;

namespace SRMCore.Services;

public class RedmineService
{
    private readonly CoreDbContext _db;
    private readonly HttpClient _http;
    private readonly ILogger<RedmineService> _logger;

    public RedmineService(CoreDbContext db, HttpClient http, ILogger<RedmineService> logger)
    {
        _db = db;
        _http = http;
        _logger = logger;
    }

    public async Task CreateTicketAsync(int comId, string subject, string description)
    {
        _logger.LogInformation("📝 Ticket-Erstellung gestartet für ComId={ComId}, Subject='{Subject}'", comId, subject);

        var redmine = await _db.Redmines.FirstOrDefaultAsync(r => r.ComId == comId);
        if (redmine == null)
        {
            _logger.LogWarning("⚠️ Kein Redmine-Eintrag für ComId={ComId} gefunden", comId);
            return;
        }

        var req = new HttpRequestMessage(HttpMethod.Post, "http://redmine:3000/issues.json");
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

        try
        {
            _logger.LogInformation("📤 Sende Ticket an Redmine: {Body}", json);
            var response = await _http.SendAsync(req);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("✅ Ticket erfolgreich an Redmine übermittelt");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Fehler beim Senden des Tickets an Redmine für ComId={ComId}", comId);
        }
    }
}
