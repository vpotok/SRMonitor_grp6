using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using SRMAuth.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Lade .env aus absolutem Pfad (für Docker) oder lokal (für VS)
var envPath = "/app/ContainerServices/.env";
if (!File.Exists(envPath))
{
    envPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, "ContainerServices", ".env");
}
Console.WriteLine($"📄 Lade .env-Datei aus: {envPath}");
Env.Load(envPath);

// 🔍 Environment-Variablen validieren
string RequireEnv(string key)
{
    var value = Environment.GetEnvironmentVariable(key);
    if (string.IsNullOrWhiteSpace(value))
    {
        Console.WriteLine($"❌ ENV {key} fehlt oder ist leer.");
        throw new Exception($"{key} fehlt");
    }
    Console.WriteLine($"✅ ENV {key} geladen.");
    return value;
}

// 📦 Konfiguration lesen
var jwtKey = RequireEnv("JWT_KEY");
var redisConnection = RequireEnv("REDIS_CONNECTION");
var key = Encoding.ASCII.GetBytes(jwtKey);

// 🔐 Authentifizierung konfigurieren
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
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(10),
        RoleClaimType = "role"
    };
});

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// 🛡️ SuperAdmin initialisieren
{
    var redis = app.Services.GetRequiredService<IConnectionMultiplexer>();
    var db = redis.GetDatabase();
    var superAdminKey = "user:superadmin";
    if (!await db.KeyExistsAsync(superAdminKey))
    {
        Console.WriteLine("🛠️ Superadmin-User wird initialisiert...");
        await db.HashSetAsync(superAdminKey, new HashEntry[]
        {
            new("username", "superadmin"),
            new("password", "supersecret123"),
            new("role", "SuperAdmin")
        });
        Console.WriteLine("✅ Superadmin erstellt.");
    }
}

// 🔑 Login
app.MapPost("/auth/login", async (HttpContext http, IConnectionMultiplexer redis, LoginRequest login) =>
{
    var db = redis.GetDatabase();
    var user = await db.HashGetAllAsync($"user:{login.Username}");
    if (user.Length == 0 || user.FirstOrDefault(e => e.Name == "password").Value != login.Password)
        return Results.Unauthorized();

    var role = user.FirstOrDefault(e => e.Name == "role").Value;

    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim("username", login.Username),
            new Claim(ClaimTypes.Role, role!)
        }),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var jwt = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    return Results.Ok(new { token = jwt });
});

// ➕ Registrierung
app.MapPost("/auth/register", [Microsoft.AspNetCore.Authorization.Authorize(Roles = "SuperAdmin")] async (
    HttpContext http,
    IConnectionMultiplexer redis,
    RegisterRequest request) =>
{
    var db = redis.GetDatabase();
    var key = $"user:{request.Username}";
    if (await db.KeyExistsAsync(key))
        return Results.Conflict("Benutzer existiert bereits.");

    await db.HashSetAsync(key, new HashEntry[]
    {
        new("username", request.Username),
        new("password", request.Password),
        new("role", request.Role)
    });

    return Results.Ok("Benutzer erfolgreich registriert.");
});

// 🔐 Token generieren
app.MapPost("/api/token", async (TokenRequest request, IConnectionMultiplexer redis) =>
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim("user_id", request.UserId.ToString()),
            new Claim("customer_id", request.CustomerId.ToString()),
            new Claim(ClaimTypes.Role, request.Role ?? "unknown")
        }),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var jwt = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

    var db = redis.GetDatabase();
    await db.StringSetAsync($"token:{jwt}", "valid", TimeSpan.FromHours(1));
    Console.WriteLine($"🔐 Token gespeichert in Redis: token:{jwt}");
    return Results.Ok(new { token = jwt });
});

// ✅ Token validieren
app.MapPost("/api/validate", [Microsoft.AspNetCore.Authorization.Authorize] async (HttpContext http, IConnectionMultiplexer redis, ClaimsPrincipal user) =>
{
    var token = http.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var db = redis.GetDatabase();

    if (!await db.KeyExistsAsync($"token:{token}"))
        return Results.Unauthorized();

    var role = user.FindFirst(ClaimTypes.Role)?.Value;
    var customerId = user.FindFirst("customer_id")?.Value;

    if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(customerId))
        return Results.Unauthorized();

    return Results.Ok(new { valid = true, role, customerId = int.Parse(customerId) });
});

// 🔓 Logout
app.MapPost("/api/logout", [Microsoft.AspNetCore.Authorization.Authorize] async (
    HttpContext http,
    IConnectionMultiplexer redis,
    ClaimsPrincipal user) =>
{
    var token = http.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var db = redis.GetDatabase();
    var deleted = await db.KeyDeleteAsync($"token:{token}");

    var username = user.FindFirst("username")?.Value ?? "unknown";
    var role = user.FindFirst(ClaimTypes.Role)?.Value ?? "unknown";
    Console.WriteLine($"👤 Logout von {username} (Rolle: {role})");

    return Results.Ok(new { success = deleted });
});

app.Run();
