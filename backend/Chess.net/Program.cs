using Chess.net.Services.Interfaces;
using Chess.net.Services;
using ChessGame.AI;
using ChessGame.GameMechanics;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder
            .WithOrigins("http://localhost:3000") // Frontend URL
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();

builder.Services.AddSignalR(o =>
{
    o.EnableDetailedErrors = true;
});

builder.Services.AddSingleton<IGameService, GameService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();
app.UseCors("CorsPolicy"); // Apply CORS
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");

app.MapHub<GameHub>("/gameHub");

app.UseRouting();

Game game = new Game(1);
game.PrintBoard();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
