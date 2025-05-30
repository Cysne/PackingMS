using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PackingService.Api.Data;
using PackingService.Api.Services;
using PackingService.Api.DTOs;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PackingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPackingService, PackingService.Api.Services.PackingService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var jwtKey = "MyVeryLongSecretKeyForJWTAuthentication123456789";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "PackingService.Api",
            ValidAudience = "PackingService.Client",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Packing Service API",
        Version = "v1",
        Description = "API para processamento de embalagem de pedidos"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer scheme. Digite 'Bearer' [espaço] e então seu token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PackingDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        await context.Database.MigrateAsync();
        logger.LogInformation(" Migrations aplicadas com sucesso!");

        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        if (!await context.Users.AnyAsync(u => u.Email == "admin@test.com"))
        {
            var adminUser = new RegisterRequestDTO
            {
                Username = "admin",
                Email = "admin@test.com",
                Password = "Admin123!"
            };
            await authService.RegisterAsync(adminUser);
            logger.LogInformation("✅ Admin user created: admin@test.com / Admin123!");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, " Erro na inicialização");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(() =>
{
    using var scope = app.Services.CreateScope();
    var packingService = scope.ServiceProvider.GetRequiredService<IPackingService>();
    packingService.InitializeBoxes();
});

app.Run();