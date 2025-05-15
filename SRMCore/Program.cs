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
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
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
using (var dbScope = app.Services.CreateScope())  // Der korrekte Name für den scope
{
    var db = dbScope.ServiceProvider.GetRequiredService<CoreDbContext>();
    var logger = dbScope.ServiceProvider.GetRequiredService<ILogger<Program>>();

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

    // Firmen erzeugen oder laden
    var companyA = db.Companies.FirstOrDefault(c => c.ComName == "AlphaTech");
    if (companyA == null)
    {
        companyA = new Company { ComName = "AlphaTech" };
        db.Companies.Add(companyA);
        logger.LogInformation("📦 Neue Firma 'AlphaTech' wird hinzugefügt.");
    }

    var companyB = db.Companies.FirstOrDefault(c => c.ComName == "BetaCorp");
    if (companyB == null)
    {
        companyB = new Company { ComName = "BetaCorp" };
        db.Companies.Add(companyB);
        logger.LogInformation("📦 Neue Firma 'BetaCorp' wird hinzugefügt.");
    }

    db.SaveChanges();
    logger.LogInformation("💾 Firmen gespeichert: AlphaTech (ID={ComIdA}), BetaCorp (ID={ComIdB})",
        companyA.ComId, companyB.ComId);

    // Nutzer hinzufügen
    if (!db.Users.Any())
    {
        logger.LogInformation("➕ Benutzer werden erstellt...");

        db.Users.AddRange(
            new User { UserName = "alpha_admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), Role = "customeradmin", ComId = companyA.ComId },
            new User { UserName = "alpha_user", PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"), Role = "customer", ComId = companyA.ComId },
            new User { UserName = "beta_admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), Role = "customeradmin", ComId = companyB.ComId },
            new User { UserName = "beta_user", PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"), Role = "customer", ComId = companyB.ComId }
        );
        db.SaveChanges();
        logger.LogInformation("✅ Benutzer gespeichert.");
    }

    // Redmine API-Key setzen
    var currentApiKey = builder.Configuration["Redmine:ApiKey"];
    if (string.IsNullOrEmpty(currentApiKey))
    {
        logger.LogError("❌ Redmine API-Key fehlt in der Konfiguration!");
        throw new Exception("Redmine API-Key fehlt in der Konfiguration (Redmine:ApiKey)");
    }

    logger.LogInformation("🔑 Redmine API-Key wird für ComId={ComId} gesetzt...", companyA.ComId);
    var existing = db.Redmines.FirstOrDefault(r => r.ComId == companyA.ComId);
    if (existing != null)
    {
        db.Redmines.Remove(existing);
        db.SaveChanges();
        logger.LogInformation("🗑️ Bestehender Redmine-Eintrag für ComId={ComId} entfernt.", companyA.ComId);
    }

    db.Redmines.Add(new Redmine
    {
        ComId = companyA.ComId,
        ApiKey = currentApiKey
    });

    try
    {
        db.SaveChanges();
        logger.LogInformation("✅ Redmine-API-Key gespeichert für ComId={ComId}.", companyA.ComId);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Fehler beim Einfügen des Redmine-API-Keys für ComId={ComId}.", companyA.ComId);
        throw;
    }

    // Agenten erzeugen
    if (!db.Agents.Any())
    {
        logger.LogInformation("🛠️ Agenten werden angelegt...");

        var agentA = new Agent
        {
            Uuid = Guid.NewGuid(),
            AuthToken = GenerateSecureToken(64),
            ComId = companyA.ComId,
            Enabled = true
        };

        var agentB = new Agent
        {
            Uuid = Guid.NewGuid(),
            AuthToken = GenerateSecureToken(64),
            ComId = companyB.ComId,
            Enabled = true
        };

        db.Agents.AddRange(agentA, agentB);

        try
        {
            db.SaveChanges();
            logger.LogInformation("✅ Agenten gespeichert.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Fehler beim Speichern der Agenten.");
            throw;
        }

        Console.WriteLine("🧾 Agent Token Übersicht:");
        Console.WriteLine($"→ {companyA.ComName} (ComId={companyA.ComId}): {agentA.AuthToken}");
        Console.WriteLine($"→ {companyB.ComName} (ComId={companyB.ComId}): {agentB.AuthToken}");
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
