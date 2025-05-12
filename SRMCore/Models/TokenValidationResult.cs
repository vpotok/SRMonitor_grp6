namespace SRMCore.Models;

public class TokenValidationResult
{
    public bool Valid { get; set; }
    public string Role { get; set; } = null!;
    public int CustomerId { get; set; }
}
