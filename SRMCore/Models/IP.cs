namespace SRMCore.Models;

public class IP
{
    public string IpAddress { get; set; } = null!;

    public int ComId { get; set; }
    public Company Company { get; set; } = null!;
}
