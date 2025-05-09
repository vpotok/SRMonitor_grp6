using SRMCore.Models;

namespace SRMCore.Services;

public class AlarmService : IAlarmService
{
    private readonly IRedmineService _redmine;

    public AlarmService(IRedmineService redmine)
    {
        _redmine = redmine;
    }

    public async Task CheckAndSendAlertAsync(ShellyData data)
    {
        var alerts = new List<string>();

        if (data.Temperature > 30) // Beispiel-Grenzwert
            alerts.Add($"Temperatur zu hoch: {data.Temperature}°C");

        if (data.DoorOpen)
            alerts.Add("Tür ist geöffnet.");

        if (alerts.Any())
        {
            var subject = "ALARM: Kritischer Zustand im Serverraum";
            var description = string.Join("\n", alerts);
            await _redmine.CreateTicketAsync(subject, description);
        }
    }
}
