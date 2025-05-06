using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SRMAuth.Services;
using Microsoft.OpenApi.Models;

namespace SRMAuth;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 🔐 1. JWT TokenService registrieren
        builder.Services.AddScoped<TokenService>();

        // 🔐 2. JWT Authentifizierung konfigurieren
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var config = builder.Configuration;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config["Jwt:Key"]!)
                    )
                };
            });

        // 📦 3. Controller & Swagger
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SRMAuth API", Version = "v1" });

    // 🔐 JWT-Schema hinzufügen
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Gib hier deinen JWT-Token ein: `Bearer <token>`"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


        var app = builder.Build();

        // 🌍 4. Swagger nur in Development aktivieren
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // 🔐 5. Authentifizierung & Autorisierung aktivieren
        //app.UseHttpsRedirection();
        app.UseAuthentication(); // <--- WICHTIG: Muss vor UseAuthorization() kommen
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
