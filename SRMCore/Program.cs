using Microsoft.EntityFrameworkCore;
using SRMCore.Data;
using SRMCore.Services;
using SRMCore.Models;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Konfiguration laden
builder.Services.AddDbContext<CoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔌 Dependency Injection (Business-Logik)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAgentAuthService, AgentAuthService>();
builder.Services.AddScoped<IAlarmService, AlarmService>();
builder.Services.AddScoped<RedmineService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITokenValidationService, TokenValidationService>();
builder.Services.AddHttpClient(); // für externe API-Aufrufe (z. B. Redmine)

// 🌐 CORS (für Frontend-Zugriffe)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// 📦 Controller & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 🧭 Middleware-Reihenfolge
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// 🗃️ Datenbankmigration & Initialdaten
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
    db.Database.Migrate();

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
}

app.Run();
