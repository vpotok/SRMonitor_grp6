namespace SRMCore.Models
{
    public class ShellyDataDto
    {
        public string ShellyId { get; set; } = null!;
        public float CurrentTemp { get; set; }
        public bool DoorOpen { get; set; }
        public DateTime KeepAliveTimestamp { get; set; }
    }
}
