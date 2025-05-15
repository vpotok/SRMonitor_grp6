using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Data.SqlClient;
using SRMCore.Data;
using SRMCore.Models;
using SRMCore.Services;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 🔑 JWT-Konfiguration
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new Exception("JWT-Key fehlt in der Konfiguration.");
var key = Encoding.ASCII.GetBytes(jwtKey);

// 🔧 Logging
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
});

// 🔐 Authentifizierung & JWT-Validierung
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true,
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddAuthorization();

// 📦 Services und DB
builder.Services.AddDbContext<CoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAgentAuthService, AgentAuthService>();
builder.Services.AddScoped<IAlarmService, AlarmService>();
builder.Services.AddScoped<RedmineService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITokenValidationService, TokenValidationService>();
builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 🔧 Middleware
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Handling request: {RequestMethod} {RequestPath}", context.Request.Method, context.Request.Path);
    await next.Invoke();
    logger.LogInformation("Finished request: {RequestMethod} {RequestPath}", context.Request.Method, context.Request.Path);
});

app.MapControllers();

// 🗃️ DB-Migration + Seeding
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        db.Database.Migrate();
        logger.LogInformation("✅ Datenbankmigration abgeschlossen.");
    }
    catch (SqlException ex) when (ex.Number == 2714) // "There is already an object named ..."
    {
        logger.LogWarning("⚠️ Migration übersprungen – Tabelle existiert bereits.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Fehler bei der Migration.");
    }


    if (!db.Companies.Any())
    {
        var companyA = new Company { ComName = "AlphaTech" };
        var companyB = new Company { ComName = "BetaCorp" };
        db.Companies.AddRange(companyA, companyB);
        db.SaveChanges();

        db.Users.AddRange(
            new User
            {
                UserName = "alpha_admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "customeradmin",
                ComId = companyA.ComId
            },
            new User
            {
                UserName = "alpha_user",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
                Role = "customer",
                ComId = companyA.ComId
            },
            new User
            {
                UserName = "beta_admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "customeradmin",
                ComId = companyB.ComId
            },
            new User
            {
                UserName = "beta_user",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
                Role = "customer",
                ComId = companyB.ComId
            }
        );
        db.SaveChanges();
    }

    if (!db.Redmines.Any())
    {
        db.Redmines.Add(new Redmine
        {
            ComId = 1,
            ApiKey = "4b2b0305ff9fc0c5549ef960310e2d5b0646ec57"
        });
        db.SaveChanges();
    }

    if (!db.Agents.Any())
    {
        db.Agents.AddRange(
            new Agent
            {
                Uuid = Guid.NewGuid(),
                AuthToken = GenerateSecureToken(64),
                ComId = 1, // AlphaTech
                Enabled = true
            },
            new Agent
            {
                Uuid = Guid.NewGuid(),
                AuthToken = GenerateSecureToken(64),
                ComId = 2, // BetaCorp
                Enabled = true
            }
        );
        db.SaveChanges();

        logger.LogInformation("✅ Agenten mit sicheren Tokens wurden angelegt.");

        // Token-Ausgabe für Debugzwecke
        var agents = db.Agents
            .OrderBy(a => a.ComId)
            .Select(a => new { a.ComId, a.AuthToken })
            .ToList();

        Console.WriteLine("🧾 Agent Token Übersicht:");
        foreach (var agent in agents)
        {
            Console.WriteLine($"→ ComId {agent.ComId}: {agent.AuthToken}");
        }
    }
}

app.Run();

// 🔐 Hilfsfunktion zur Token-Generierung
static string GenerateSecureToken(int length)
{
    const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
    var random = new Random();
    return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
}