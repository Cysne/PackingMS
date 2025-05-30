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
    .WriteTo.File("../logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();


builder.Services.AddDbContext<PackingDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtKey = builder.Configuration["Jwt:Key"] ?? "SuperSecretKeyWithAtLeast32Characters123!";
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
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddScoped<IPackingService, PackingService.Api.Services.PackingService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IPackingStrategy, FirstFitDecreasingPackingStrategy>();
builder.Services.AddSingleton<IEnumerable<BoxDTO>>(sp => new[]
{
    new BoxDTO { BoxType = "Caixa 1", Height = 30m, Width = 40m, Length = 80m },
    new BoxDTO { BoxType = "Caixa 2", Height = 80m, Width = 50m, Length = 40m },
    new BoxDTO { BoxType = "Caixa 3", Height = 50m, Width = 80m, Length = 60m }
});


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
    db.Database.Migrate();
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