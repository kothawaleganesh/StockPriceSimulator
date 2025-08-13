using StockPriceSimulator.Hubs;
using StockPriceSimulator.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Registers SignalR Service
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddHostedService<StockPriceSimulatorService>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(opt =>
        {
            opt.AddPolicy("CorsPolicy",
            policy => policy.AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins("http://localhost:3000")
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true)
            );
        });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthorization();
app.MapControllers();
app.MapHub<StockHub>("/stockhub");
app.Run();
