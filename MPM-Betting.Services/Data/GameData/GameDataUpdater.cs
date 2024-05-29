using Microsoft.Extensions.Hosting;

namespace MPM_Betting.Services.Data.GameData;

public class GameDataUpdater(GameDataUpdateScheduler scheduler) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        scheduler.FootballGames.TryAdd(4193903, 500);
        return Task.CompletedTask;
    }
}