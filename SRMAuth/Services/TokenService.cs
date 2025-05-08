using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SRMAuth.Services;

public class TokenService
{
    private readonly IConnectionMultiplexer _redis;

    public TokenService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public string GenerateToken(string username, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("0101YourSuperSecretKeyHereThatIsAtLeast32CharsLong0101"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "SRMAuth",
            audience: "SRMCore",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        // Log the token in Redis
        var db = _redis.GetDatabase();
        db.StringSet($"token:{username}", tokenString, TimeSpan.FromHours(1));

        return tokenString;
    }
}