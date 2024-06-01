using System.Net.Mail;
using MPM_Betting.DataModel;
using MPM_Betting.Services;
using MPM_Betting.Services.Data.BetManager;
using MPM_Betting.Services.Data.GameData;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddServiceDefaults();
builder.AddMpmDbContext();
builder.AddMpmCache();
builder.AddMpmAuth();
builder.AddMpmMail();
builder.AddFootballApi();

builder.Services.AddSingleton<BetProcessor>();
builder.Services.AddSingleton<GameDataUpdater>();
builder.Services.AddSingleton<GameDataUpdateScheduler>();
builder.Services.AddSingleton<GameDataQueueWorker>();
builder.Services.AddSingleton<IBackgroundTaskQueue>(_ => new GameDataUpdateQueue(100));
builder.Services.AddHostedService(services => services.GetRequiredService<BetProcessor>());
builder.Services.AddHostedService(services => services.GetRequiredService<GameDataUpdater>());
builder.Services.AddHostedService(services => services.GetRequiredService<GameDataUpdateScheduler>());
builder.Services.AddHostedService(services => services.GetRequiredService<GameDataQueueWorker>());

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapAuthEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapFootballEndpoints();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/testMail", async (SmtpClient smtpClient, string email) =>
{
    using var message = new MailMessage("no-reply@mpm-betting.at", email);
    message.Subject = "Take this email and wear it with proud";
    message.Body = "some data, <h1>HTML works?</h1><h2>Who knows</h2>";
    message.IsBodyHtml = true;
    await smtpClient.SendMailAsync(message);
});

app.MapGet("/weatherforecast", () =>
    {
        Thread.Sleep(2000);
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();
