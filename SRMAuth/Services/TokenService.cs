using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SRMAuth.Services;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(int userId, string role, int customerId)
{
    var claims = new[]
    {
        new Claim("user_id", userId.ToString()),
        new Claim("role", role),
        new Claim("customer_id", customerId.ToString())
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        expires: DateTime.UtcNow.AddHours(1),
        claims: claims,
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}

}
