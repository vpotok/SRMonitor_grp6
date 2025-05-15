using SRMCore.Data;
using SRMCore.Models;
using Microsoft.Extensions.Logging;

namespace SRMCore.Services;

public class AlarmService : IAlarmService
{
    private readonly CoreDbContext _db;
    private readonly RedmineService _redmine;
    private readonly ILogger<AlarmService> _logger;

    public AlarmService(CoreDbContext db, RedmineService redmine, ILogger<AlarmService> logger)
    {
        _db = db;
        _redmine = redmine;
        _logger = logger;
    }

    public async Task CheckAndTriggerRedmineIfNeededAsync(int comId, ShellyDataDto data)
    {
        var messages = new List<string>();

        if (data.CurrentTemp > 30)
            messages.Add($"Temperatur über 30°C: {data.CurrentTemp}°C");

        if (data.DoorOpen)
            messages.Add("Serverraumtür ist geöffnet");

        if (!messages.Any())
        {
            _logger.LogInformation("ℹ️ Keine Alarmbedingungen erfüllt für ComId={ComId}", comId);
            return;
        }

        var subject = "ALARM: Serverraumzustand kritisch";
        var description = string.Join("\\n", messages);

        _logger.LogInformation("Alarmbedingungen erfüllt – Trigger Redmine für ComId={ComId}", comId);
        await _redmine.CreateTicketAsync(comId, subject, description);
    }
}
