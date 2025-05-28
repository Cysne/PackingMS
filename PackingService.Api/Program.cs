using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PackingService.Api.Data;
using PackingService.Api.DTOs;
using PackingService.Api.Services;
using PackingService.Api.Strategies;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<PackingDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IPackingService, PackingService.Api.Services.PackingService>();
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
});

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PackingDbContext>();
    db.Database.Migrate();
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Packing API v1");
        c.DocExpansion(DocExpansion.List);
    });
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();