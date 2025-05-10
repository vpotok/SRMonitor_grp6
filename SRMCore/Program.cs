using SRMCore.Services;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:8080") // Allow SRMApp frontend
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Allow cookies and credentials
        });
});

// Register AuthService
builder.Services.AddScoped<AuthService>();
builder.Services.AddHttpClient<AuthService>();

var app = builder.Build();

// Use JWT Middleware
app.UseMiddleware<SRMCore.Middleware.JwtMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.MapControllers();
app.Run();