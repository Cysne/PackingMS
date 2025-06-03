using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PackingService.Api.Data;
using PackingService.Api.DTOs;
using PackingService.Api.Services;
using PackingService.Api.Strategies;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("../logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();


builder.Services.AddDbContext<PackingDbContext>(opts =>
{
    if (builder.Environment.IsDevelopment())
    {
        opts.UseInMemoryDatabase("PackingDb");
    }
    else
    {
        opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});

// Configuração JWT padronizada usando IConfiguration
var jwtKey = builder.Configuration["JwtSettings:SecretKey"] ?? builder.Configuration["Jwt:Key"] ?? "SuperSecretKeyWithAtLeast32Characters123!";
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? builder.Configuration["Jwt:Issuer"] ?? "PackingService.Api";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? builder.Configuration["Jwt:Audience"] ?? "PackingService.Api";

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddScoped<IPackingService, PackingService.Api.Services.PackingService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<PackingService.Api.Services.ITransactionService, PackingService.Api.Services.TransactionService>();
builder.Services.AddSingleton<IPackingStrategy, FirstFitDecreasingPackingStrategy>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Packing API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
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

app.UseMiddleware<PackingService.Api.Middleware.ExceptionMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PackingDbContext>();

    if (db.Database.IsRelational())
    {
        db.Database.Migrate();
    }
    else
    {
        db.Database.EnsureCreated();
    }

    db.SeedData();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Packing API v1");
    c.DocExpansion(DocExpansion.List);
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();