namespace SRMCore.Models;

public class ShellyData
{
    public int Id { get; set; }
    public string Mac { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public double Temperature { get; set; }
    public bool DoorOpen { get; set; }
    public double? Lux { get; set; }
    public double? Battery { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
}
