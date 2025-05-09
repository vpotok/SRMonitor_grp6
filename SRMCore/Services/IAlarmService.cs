using SRMCore.Models;

namespace SRMCore.Services;

public interface IAlarmService
{
    Task CheckAndSendAlertAsync(ShellyData data);
}
