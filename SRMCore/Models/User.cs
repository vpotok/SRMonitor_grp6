namespace SRMCore.Models;

public class User
{
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = null!; // "customer" oder "customeradmin"

    public int ComId { get; set; }
    public Company Company { get; set; } = null!;
}
