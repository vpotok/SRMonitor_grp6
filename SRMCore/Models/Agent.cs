namespace SRMCore.Models;

public class Agent
{
    public string AuthToken { get; set; } = null!; // PK & Geheim-Token
    public bool Enabled { get; set; }

    public int ComId { get; set; }
    public Company Company { get; set; } = null!;
}
