using Microsoft.EntityFrameworkCore;
using SRMCore.Data;
using SRMCore.Services;

var builder = WebApplication.CreateBuilder(args);

// Konfiguration laden
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                     .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                     .AddEnvironmentVariables();

// Datenbankkontext
builder.Services.AddDbContext<CoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Eigene Services
builder.Services.AddHttpClient<ITokenService, TokenService>();
builder.Services.AddScoped<IAlarmService, AlarmService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpClient<ITokenValidationService, TokenValidationService>();
builder.Services.AddHttpClient<IRedmineService, RedmineService>();



// API + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
