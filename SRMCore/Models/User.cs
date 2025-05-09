namespace SRMCore.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = "Customer"; // "Customer", "Employee", "Agent"
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
}
