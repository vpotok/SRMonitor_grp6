namespace SRMCore.Models;

public class PingResultDto
{
    public string IpAddress { get; set; } = null!;
    public bool Success { get; set; }
    public long ResponseTimeMs { get; set; }
}
