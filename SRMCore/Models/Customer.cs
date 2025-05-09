namespace SRMCore.Models;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<ShellyData> ShellyData { get; set; } = new List<ShellyData>();
}
