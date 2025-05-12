using SRMCore.Models;

namespace SRMCore.Services;

public interface IAlarmService
{
    Task CheckAndTriggerRedmineIfNeededAsync(int comId, ShellyDataDto data);
}
