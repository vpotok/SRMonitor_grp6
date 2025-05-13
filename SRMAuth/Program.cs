using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using SRMAuth.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Konfiguration laden
var configuration = builder.Configuration;
var jwtKey = configuration["Jwt:Key"] ?? throw new Exception("JWT-Key fehlt in der Konfiguration.");
var redisConnection = configuration.GetSection("Redis")["ConnectionString"] ?? "redis:6379";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
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

// 🔐 SuperAdmin beim Start setzen
var redis = app.Services.GetRequiredService<IConnectionMultiplexer>();
var db = redis.GetDatabase();

var superAdminUsername = configuration["SuperAdmin:Username"] ?? "superadmin";
var superAdminPassword = configuration["SuperAdmin:Password"] ?? "supersecret123";
var superAdminRole = configuration["SuperAdmin:Role"] ?? "SuperAdmin";

var superAdminKey = $"user:{superAdminUsername}";
var superAdminHash = new HashEntry[]
{
    new("username", superAdminUsername),
    new("password", superAdminPassword),
    new("role", superAdminRole)
};
await db.HashSetAsync(superAdminKey, superAdminHash);

app.UseAuthentication();
app.UseAuthorization();

// 🔐 Login
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
    var token = tokenHandler.CreateToken(tokenDescriptor);
    var jwt = tokenHandler.WriteToken(token);

    return Results.Ok(new { token = jwt });
});

// ➕ Benutzer registrieren (nur für SuperAdmin erlaubt)
app.MapPost("/auth/register", [Microsoft.AspNetCore.Authorization.Authorize(Roles = "SuperAdmin")] async (HttpContext http, IConnectionMultiplexer redis, RegisterRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Role))
        return Results.BadRequest("Username, Password und Role müssen angegeben werden.");

    var db = redis.GetDatabase();
    var key = $"user:{request.Username}";

    if (await db.KeyExistsAsync(key))
        return Results.Conflict("Benutzer existiert bereits.");

    var hash = new HashEntry[]
    {
        new("username", request.Username),
        new("password", request.Password),
        new("role", request.Role)
    };

    await db.HashSetAsync(key, hash);
    return Results.Ok("Benutzer erfolgreich registriert.");
});

// Token erstellen + in Redis speichern
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
        NotBefore = DateTime.UtcNow.AddSeconds(-5),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var jwt = tokenHandler.WriteToken(token);

    var db = redis.GetDatabase();
    await db.StringSetAsync($"token:{jwt}", "valid", TimeSpan.FromHours(1));
    Console.WriteLine($"🔐 Token gespeichert in Redis: token:{jwt}");

    return Results.Ok(new { token = jwt });
});

// Token validieren + Redis check
app.MapPost("/api/validate", [Microsoft.AspNetCore.Authorization.Authorize] async (HttpContext http, IConnectionMultiplexer redis, ClaimsPrincipal user) =>
{
    var authHeader = http.Request.Headers["Authorization"].ToString();
    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
    {
        Console.WriteLine("❌ Kein Authorization-Header oder falsches Format.");
        return Results.Unauthorized();
    }

    var token = authHeader.Replace("Bearer ", "");
    var db = redis.GetDatabase();
    var exists = await db.KeyExistsAsync($"token:{token}");
    Console.WriteLine(exists ? "✅ Token in Redis gefunden." : "❌ Token nicht in Redis vorhanden.");
    if (!exists)
        return Results.Unauthorized();

    var userId = user.FindFirst("user_id")?.Value;
    var role = user.FindFirst(ClaimTypes.Role)?.Value;
    var customerId = user.FindFirst("customer_id")?.Value;

    Console.WriteLine($"🔍 Claims: user_id={userId}, role={role}, customer_id={customerId}");

    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role) || string.IsNullOrEmpty(customerId))
        return Results.Unauthorized();

    return Results.Ok(new
    {
        userId,
        role,
        customerId
    });
});
// 🔓 Token löschen (Logout) – abgesichert mit [Authorize]
app.MapPost("/api/logout", [Microsoft.AspNetCore.Authorization.Authorize] async (
    HttpContext http,
    IConnectionMultiplexer redis,
    ClaimsPrincipal user) =>
{
    var authHeader = http.Request.Headers["Authorization"].ToString();
    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        return Results.BadRequest("Kein oder ungültiger Authorization-Header.");

    var token = authHeader.Replace("Bearer ", "");
    var db = redis.GetDatabase();
    var deleted = await db.KeyDeleteAsync($"token:{token}");

    Console.WriteLine(deleted
        ? $"🧹 Token gelöscht: token:{token}"
        : $"⚠️ Token war nicht vorhanden: token:{token}");

    // Optional: logge auch den Benutzername oder Role aus dem Token
    var username = user.FindFirst("username")?.Value ?? "unknown";
    var role = user.FindFirst(ClaimTypes.Role)?.Value ?? "unknown";
    Console.WriteLine($"👤 Logout: user={username}, role={role}");

    return Results.Ok(new { success = deleted });
});

app.Run();
