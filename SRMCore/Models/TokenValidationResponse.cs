namespace SRMCore.Models;

public class TokenValidationResponse
{
    public bool Valid { get; set; }
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public int CustomerId { get; set; }
}
