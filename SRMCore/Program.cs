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
using DotNetEnv;
DotNetEnv.Env.Load(Path.Combine("..", "ContainerServices", ".env"));

Console.WriteLine("🟡 Starte Initialisierung...");

Console.WriteLine("📄 Lade .env-Datei...");
Console.WriteLine("✅ .env geladen.");

// Debug-Ausgabe aller kritischen ENV-Werte
string[] keys = {
    "CORE_DB_CONNECTION", "JWT_KEY", "TOKEN_BASE_URL",
    "REDMINE_BASE_URL", "REDMINE_API_KEY"
};

foreach (var keyName in keys)
{
    var value = Environment.GetEnvironmentVariable(keyName);
    Console.WriteLine($"🔍 ENV: {keyName} = {(string.IsNullOrEmpty(value) ? "❌ NICHT GESETZT" : "✅ geladen")}");
}

// Builder starten
var builder = WebApplication.CreateBuilder(args);
Console.WriteLine("🔧 WebApplicationBuilder initialisiert.");

// Konfig aus ENV übernehmen
Console.WriteLine("⚙️ Übernehme Konfigurationen...");

builder.Configuration["ConnectionStrings:DefaultConnection"] =
    Environment.GetEnvironmentVariable("CORE_DB_CONNECTION");

builder.Configuration["Jwt:Key"] =
    Environment.GetEnvironmentVariable("JWT_KEY");

builder.Configuration["TokenService:BaseUrl"] =
    Environment.GetEnvironmentVariable("TOKEN_BASE_URL");

builder.Configuration["Redmine:BaseUrl"] =
    Environment.GetEnvironmentVariable("REDMINE_BASE_URL");

builder.Configuration["Redmine:ApiKey"] =
    Environment.GetEnvironmentVariable("REDMINE_API_KEY");

Console.WriteLine("✅ Konfiguration übernommen.");

// JWT Key validieren
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    Console.WriteLine("❌ Fehler: JWT-Key konnte nicht geladen werden.");
    throw new Exception("JWT-Key fehlt.");
}
Console.WriteLine("🔑 JWT-Key geladen.");

// JWT Key vorbereiten
var key = Encoding.ASCII.GetBytes(jwtKey);

// Logging initialisieren
builder.Services.AddLogging(loggingBuilder => {
    loggingBuilder.AddConsole();
});

Console.WriteLine("📦 Logging aktiviert.");

// Authentifizierung mit JWT
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})

.AddJwtBearer(options => {
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true,
        RoleClaimType = ClaimTypes.Role
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // If no token in header, try to get it from the cookie
            if (string.IsNullOrEmpty(context.Token))
            {
                context.Token = context.Request.Cookies["jwt"];
            }
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();
Console.WriteLine("🔐 Authentifizierung & Autorisierung registriert.");

// DB
builder.Services.AddDbContext<CoreDbContext>(options => {
    var conn = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(conn))
    {
        Console.WriteLine("❌ CORE_DB_CONNECTION fehlt oder leer.");
        throw new Exception("CORE_DB_CONNECTION fehlt.");
    }
    Console.WriteLine("🛢️ Datenbankverbindung geladen.");
    options.UseSqlServer(conn);
});

// Services
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
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Console.WriteLine("✅ Services und Middleware konfiguriert.");

// App bauen
var app = builder.Build();
Console.WriteLine("🟢 Anwendung gebaut.");

// Middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) => {
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("➡️ Anfrage: {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
    logger.LogInformation("✅ Antwort: {Method} {Path}", context.Request.Method, context.Request.Path);
});

app.MapControllers();

Console.WriteLine("🧱 Starte DB-Migration & Seeding...");
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        db.Database.Migrate();
        logger.LogInformation("✅ Migration abgeschlossen.");
    }
    catch (SqlException ex) when (ex.Number == 2714)
    {
        logger.LogWarning("⚠️ Migration übersprungen – Objekt existiert bereits.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Fehler bei der Migration.");
    }

    // Firmen
    var companyA = db.Companies.FirstOrDefault(c => c.ComName == "AlphaTech") ?? new Company { ComName = "AlphaTech" };
    var companyB = db.Companies.FirstOrDefault(c => c.ComName == "BetaCorp") ?? new Company { ComName = "BetaCorp" };
    if (companyA.ComId == 0) db.Companies.Add(companyA);
    if (companyB.ComId == 0) db.Companies.Add(companyB);
    db.SaveChanges();
    logger.LogInformation("💾 Firmen gespeichert: {A}, {B}", companyA.ComId, companyB.ComId);

    // User
    if (!db.Users.Any())
    {
    
        var adminPw = Environment.GetEnvironmentVariable("DEFAULT_ADMIN_PASSWORD");
        var userPw = Environment.GetEnvironmentVariable("DEFAULT_USER_PASSWORD");

        if (string.IsNullOrEmpty(adminPw) || string.IsNullOrEmpty(userPw))
        {
            throw new Exception("⚠️ Standardpasswörter nicht gesetzt. Bitte .env prüfen.");
        }

        db.Users.AddRange(
            new User { UserName = "alpha_admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPw), Role = "customeradmin", ComId = companyA.ComId },
            new User { UserName = "alpha_user", PasswordHash = BCrypt.Net.BCrypt.HashPassword(userPw), Role = "customer", ComId = companyA.ComId },
            new User { UserName = "beta_admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPw), Role = "customeradmin", ComId = companyB.ComId },
            new User { UserName = "beta_user", PasswordHash = BCrypt.Net.BCrypt.HashPassword(userPw), Role = "customer", ComId = companyB.ComId }
        );
        db.SaveChanges();
        logger.LogInformation("✅ Benutzer gespeichert.");
    }
    // 🔐 Redmine-API-Key automatisch abrufen
    var redmineBaseUrl = builder.Configuration["Redmine:BaseUrl"];
    var redmineUsername = "admin";
    var redminePasswordFile = "/init-scripts/admin.txt";

    Console.WriteLine("🟠 Starte Redmine-API-Key-Abruf...");

    if (!File.Exists(redminePasswordFile))
    {
        Console.WriteLine($"❌ Redmine-Passwortdatei fehlt: {redminePasswordFile}");
        throw new FileNotFoundException("Redmine-Passwortdatei nicht gefunden", redminePasswordFile);
    }

    var redminePassword = File.ReadAllText(redminePasswordFile).Trim();
    Console.WriteLine("✅ Passwortdatei gefunden und geladen.");

    // HTTP-Client vorbereiten
    using var redmineClient = new HttpClient();
    var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{redmineUsername}:{redminePassword}"));
    redmineClient.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

    const int maxAttempts = 5;
    bool success = false;

    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        Console.WriteLine($"🌐 Versuche Verbindung zu Redmine ({attempt}/{maxAttempts})...");

        try
        {
            var url = $"{redmineBaseUrl}/users/current.json";
            Console.WriteLine($"🔗 Request an: {url}");

            var response = await redmineClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var json = System.Text.Json.JsonDocument.Parse(jsonString);
                var apiKeyFromRedmine = json.RootElement.GetProperty("user").GetProperty("api_key").GetString();

                if (string.IsNullOrWhiteSpace(apiKeyFromRedmine))
                {
                    Console.WriteLine("❌ API-Key im JSON nicht gefunden!");
                    throw new Exception("API-Key nicht im Antwort-JSON gefunden");
                }

                builder.Configuration["Redmine:ApiKey"] = apiKeyFromRedmine;
                Console.WriteLine($"✅ Redmine-API-Key geladen: {apiKeyFromRedmine}");
                // Redmine-Eintrag in DB speichern
var redmineEntry = db.Redmines.FirstOrDefault(r => r.ComId == companyA.ComId);
if (redmineEntry != null)
{
    db.Redmines.Remove(redmineEntry);
    db.SaveChanges();
}

db.Redmines.Add(new Redmine
{
    ComId = companyA.ComId,
    ApiKey = apiKeyFromRedmine
});
db.SaveChanges();
logger.LogInformation("✅ Redmine-API-Key gespeichert in DB für ComId={ComId}.", companyA.ComId);

                success = true;
                break;
            }
            else
            {
                Console.WriteLine($"⚠️ Fehler: HTTP {response.StatusCode}, Inhalt: {await response.Content.ReadAsStringAsync()}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ausnahme beim API-Call: {ex.Message}");
        }

        await Task.Delay(3000); // ⏳ Warten vor erneutem Versuch
    }

    if (!success)
    {
        Console.WriteLine("❌ Redmine-API-Key konnte nach mehreren Versuchen nicht geladen werden.");
        throw new Exception("Redmine-API-Key konnte nicht abgerufen werden.");
    }
    
    // 🧾 Agenten anlegen (falls nicht vorhanden)
    if (!db.Agents.Any())
    {
        logger.LogInformation("🛠️ Agenten werden erstellt...");

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
        db.SaveChanges();

        logger.LogInformation("✅ Agenten gespeichert.");
        Console.WriteLine("🧾 Agent Token Übersicht:");
        Console.WriteLine($"→ {companyA.ComName} (ComId={companyA.ComId}): {agentA.AuthToken}");
        Console.WriteLine($"→ {companyB.ComName} (ComId={companyB.ComId}): {agentB.AuthToken}");
    }
    static string GenerateSecureToken(int length)
{
    const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
    var random = new Random();
    return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
}

}
app.Run(); // ⬅️ das ist wichtig! Damit läuft der Webserver dauerhaft
