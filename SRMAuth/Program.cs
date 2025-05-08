using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using SRMAuth.Models;


var builder = WebApplication.CreateBuilder(args);

// JWT Config
var key = Encoding.ASCII.GetBytes("SuperSecretKey-ChangeThis-UseAtLeast32Bytes!");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("redis:6379"));

var app = builder.Build();

// Seed Agent user
var redis = app.Services.GetRequiredService<IConnectionMultiplexer>();
    var db = redis.GetDatabase();
        if (!db.KeyExists("user:agent"))
        {
            var hash = new HashEntry[]
            {
                new("username", "agent"),
                new("password", "password123"),
                new("role", "Agent")
            };
            db.HashSet("user:agent", hash);
        }

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/auth/login", async (HttpContext http, IConnectionMultiplexer redis, LoginRequest login) =>
{
    var db = redis.GetDatabase();
    var user = await db.HashGetAllAsync($"user:{login.Username}");
    if (user.Length == 0 || user.FirstOrDefault(e => e.Name == "password").Value != login.Password)
        return Results.Unauthorized();

    var role = user.FirstOrDefault(e => e.Name == "role").Value;
    var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new System.Security.Claims.ClaimsIdentity(new[]
        {
            new System.Security.Claims.Claim("username", login.Username),
            new System.Security.Claims.Claim("role", role!)
        }),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    var jwt = tokenHandler.WriteToken(token);

    return Results.Ok(new { token = jwt });
});

app.MapGet("/telemetry", [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Agent")] () =>
{
    return Results.Ok("Telemetry data for Agent only");
});

app.Run();

