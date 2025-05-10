using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace SRMAuth.Services;

public class TokenService
{
    private readonly IConnectionMultiplexer _redis;
    private const string SecretKey = "0101YourSuperSecretKeyHereThatIsAtLeast32CharsLong0101";
    private const string Issuer = "SRMAuth";
    private const string Audience = "SRMCore";

    public TokenService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public string GenerateToken(string username, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(2),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return tokenString; // Return only the raw JWT token
    }

    public string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(SecretKey);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                ValidateLifetime = true
            }, out _);

            return principal; // Return the ClaimsPrincipal if the token is valid
        }
        catch
        {
            return null; // Return null if the token is invalid
        }
    }
    public async Task StoreRefreshToken(string username, string refreshToken)
    {
        var db = _redis.GetDatabase();
        var hashedToken = BCrypt.Net.BCrypt.HashPassword(refreshToken); // Hash the token
        await db.HashSetAsync("refresh_tokens", new HashEntry[] { new HashEntry(hashedToken, username) });
    }

    public async Task<string?> ValidateRefreshToken(string refreshToken)
    {
        var db = _redis.GetDatabase();
        var hashEntries = await db.HashGetAllAsync("refresh_tokens");

        foreach (var entry in hashEntries)
        {
            if (BCrypt.Net.BCrypt.Verify(refreshToken, entry.Name))
            {
                return entry.Value; // Return the associated username
            }
        }

        return null; // Return null if no match is found
    }

    public async Task InvalidateRefreshToken(string refreshToken)
    {
        var db = _redis.GetDatabase();
        var hashEntries = await db.HashGetAllAsync("refresh_tokens");

        foreach (var entry in hashEntries)
        {
            if (BCrypt.Net.BCrypt.Verify(refreshToken, entry.Name))
            {
                await db.HashDeleteAsync("refresh_tokens", entry.Name); // Delete the matching entry
                break;
            }
        }
    }
}