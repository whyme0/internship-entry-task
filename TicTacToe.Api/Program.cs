using Microsoft.EntityFrameworkCore;
using TicTacToe.Api.Domain.Data;
using TicTacToe.Api.Domain.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddNewtonsoftJson(o =>
    {
        o.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
        o.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include;
    });
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddMemoryCache();

builder.Services.AddScoped<IGameRepository, GameRepository>();

var app = builder.Build();

using(var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () =>
{
    return Results.Ok();
});

app.Run();
