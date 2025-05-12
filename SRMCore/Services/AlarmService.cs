using SRMCore.Data;
using SRMCore.Models;

namespace SRMCore.Services;

public class AlarmService : IAlarmService
{
    private readonly CoreDbContext _db;
    private readonly RedmineService _redmine;

    public AlarmService(CoreDbContext db, RedmineService redmine)
    {
        _db = db;
        _redmine = redmine;
    }

    public async Task CheckAndTriggerRedmineIfNeededAsync(int comId, ShellyDataDto data)
    {
        var messages = new List<string>();

        if (data.Temperature > 30)
            messages.Add($"Temperatur über 30°C: {data.Temperature}°C");

        if (data.DoorOpen)
            messages.Add("Serverraumtür ist geöffnet");

        if (!messages.Any()) return;

        var subject = "🚨 ALARM: Serverraumzustand kritisch";
        var description = string.Join("\\n", messages);

        await _redmine.CreateTicketAsync(comId, subject, description);
    }
}
