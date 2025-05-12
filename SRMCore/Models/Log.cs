namespace SRMCore.Models;

public class Log
{
    public int LogId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = null!;
    public string Type { get; set; } = null!; // "shelly" oder "ip"

    public int ComId { get; set; }
    public Company Company { get; set; } = null!;
}
