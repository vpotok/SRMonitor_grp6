namespace SRMCore.Models;

public class Company
{
    public int ComId { get; set; }
    public string ComName { get; set; } = null!;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Agent> Agents { get; set; } = new List<Agent>();
    public ICollection<IP> IPs { get; set; } = new List<IP>();
    public ICollection<Log> Logs { get; set; } = new List<Log>();
    public Redmine? Redmine { get; set; }
}
