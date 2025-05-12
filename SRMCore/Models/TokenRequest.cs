namespace SRMCore.Models;

public class TokenRequest
{
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public int CustomerId { get; set; }
};

