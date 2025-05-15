namespace SRMCore.Models;

public class Redmine
{
    public int ComId { get; set; }
    public string ApiKey { get; set; } = null!;

    public Company Company { get; set; } = null!;
}
