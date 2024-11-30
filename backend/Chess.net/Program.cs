using Domain.Users;
using Infrastructure.DataContext;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

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
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
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
/*builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("")
});*/

builder.Services.AddDbContext<DomainDataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.Cookie.Name = "UserAuthCookie";
        options.LoginPath = "/Identity/Account/Login"; // Domyœlny endpoint logowania
    });
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<User>()
    .AddEntityFrameworkStores<DomainDataContext>();

var app = builder.Build();
app.UseCors("AllowSpecificOrigin");

//builder.Logging.ClearProviders();
builder.Logging.AddConsole();

app.UseCors("CorsPolicy"); // Apply CORS
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapIdentityApi<User>();

app.UseCors("AllowReactApp");

app.MapHub<GameHub>("/gameHub");

app.UseRouting();

Game game = new Game(1);
game.PrintBoard();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
