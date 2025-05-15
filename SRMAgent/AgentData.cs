namespace SRMAgent;

public class AgentData
{
    public string ShellyId { get; set; } = "";

    public float CurrentHumidity { get; set; } = 0;

    public float Lux { get; set; } = 0;
    
    public float CurrentTemp { get; set; } = 0;
    public bool DoorOpen { get; set; } = false;
    public DateTime KeepAliveTimestamp { get; set; } = DateTime.MinValue;
}
