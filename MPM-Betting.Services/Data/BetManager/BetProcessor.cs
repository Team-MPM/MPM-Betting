using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MPM_Betting.DataModel;
using MPM_Betting.DataModel.Football;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.Services.Data.BetManager;

public class BetProcessor(
    ILogger<BetProcessor> logger,
    FootballApi footballApi,
    IDbContextFactory<MpmDbContext> dbContextFactory)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BetProcessor Service running");

        // When the timer should have no due-time, then do the work once now.
        await ProcessBets();

        using PeriodicTimer timer = new(TimeSpan.FromSeconds(30));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                Console.WriteLine("BetProcessor Tick");
                await ProcessBets();
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("BetProcessor Service is stopping");
        }
    }

    private async Task ProcessBets()
    {
        var db = await dbContextFactory.CreateDbContextAsync();
        
        var resultBets = await db.FootballGameBets
            .Where(b => b.Processed == false && b.Game.GameState == EGameState.Finished)
            .Include(bet => bet.Game)
            .Include(bet => bet.Group)
            .Include(bet => bet.User)
            .ToListAsync();

        foreach (var bet in resultBets)
        {
            var gameId = bet.Game.ReferenceId;
            var gameDataResult = await footballApi.GetGameDetails(gameId);
            
            if (!gameDataResult.IsSuccess) continue;
            
            var gameData = gameDataResult.Value;
            
            // if (GameDataUpdateScheduler.FootballApiGameStateToEGameState(gameData.GameEntry.Score.State) is not
            //     EGameState.Finished)
            // {
            //     logger.LogError("Game {GameId} is not finished yet but marked as finished", gameId);
            //     return;
            // }
            //
            // if (GameDataUpdateScheduler.FootballApiGameStateToEGameState(gameData.GameEntry.Score.State) is EGameState.Cancelled)
            // {
            //     bet.Processed = true;
            //     bet.Completed = false;
            //     await db.SaveChangesAsync();
            //     return;
            // }
                
            var home = gameData.GameEntry.Score.HomeScore;
            var away = gameData.GameEntry.Score.AwayScore;

            var quote = bet.Quote;
                
            if (home == away)
            {
                // draw
                if (bet.Result is EResult.Draw)
                {
                    bet.Hit = true;
                    bet.Completed = true;
                }
            }
            else if (home < away)
            {
                // away win
                if (bet.Result is EResult.Loss)
                {
                    bet.Hit = true;
                    bet.Completed = true;
                }
            }
            else
            {
                // home win
                if (bet.Result is EResult.Win)
                {
                    bet.Hit = true;
                    bet.Completed = true;
                }
            }

            if (bet.Group is MpmGroup group)
            {
                // Group bet
                var uge = await db.UserGroupEntries
                    .Where(uge => uge.MpmUserId == bet.User.Id && uge.GroupId == group.Id)
                    .FirstOrDefaultAsync();

                if (uge is null)
                {
                    await db.SaveChangesAsync();
                    logger.LogError("Game {GameId} has a group bet but user is not part of group", gameId);
                    return;
                }
                    
                uge.Score += (int)(bet.Points * quote);
            }
            else
            {
                // Global bet
                bet.User.Points += (int)(bet.Points * quote);
            }
                
            bet.Processed = true;
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
        }
        
        // TODO
        // var scoreBets = await db.FootballScoreBets
        //     .Where(b => b.Processed == false && b.Game.GameState == EGameState.Finished)
        //     .ToListAsync();
    }
}