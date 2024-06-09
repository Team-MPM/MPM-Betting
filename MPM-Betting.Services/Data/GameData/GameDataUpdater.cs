using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MPM_Betting.DataModel;
using MPM_Betting.DataModel.Betting;
using MPM_Betting.DataModel.Football;

namespace MPM_Betting.Services.Data.GameData;

public class GameDataUpdater(IDbContextFactory<MpmDbContext> dbContextFactory, GameDataUpdateScheduler scheduler) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var db = await dbContextFactory.CreateDbContextAsync(stoppingToken);

        using PeriodicTimer timer = new(TimeSpan.FromSeconds(5));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var upcomingAndRunningGames = db.Games.Where(g => g.GameState != EGameState.Finished && g.SportType == ESportType.Football);
            foreach (var game in upcomingAndRunningGames)
            {
                scheduler.FootballGames.TryAdd(game.ReferenceId, 10);
            }
        }
    }
}