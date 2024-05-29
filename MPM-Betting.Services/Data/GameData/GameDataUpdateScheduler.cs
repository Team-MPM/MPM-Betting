using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MPM_Betting.DataModel;
using MPM_Betting.DataModel.Betting;
using MPM_Betting.DataModel.Football;

namespace MPM_Betting.Services.Data.GameData;

public class GameDataUpdateScheduler(
    ILogger<GameDataUpdateScheduler> logger, 
    IServiceProvider serviceProvider,
    FootballApi footballApi,
    IBackgroundTaskQueue gameDataUpdateQueue,
    IDbContextFactory<MpmDbContext> dbContextFactory) 
    : BackgroundService
{
    public ConcurrentDictionary<int, int> FootballLeagues { get; set; } = new();
    public ConcurrentDictionary<int, int> FootballGames { get; set; } = new();

    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Timed Hosted Service running");

        // When the timer should have no due-time, then do the work once now.
        await DoWork();

        using PeriodicTimer timer = new(TimeSpan.FromSeconds(5));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                Console.WriteLine("Tick");
                await DoWork();
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Timed Hosted Service is stopping");
        }
    }

    public class FootballApiException : Exception;
    private static readonly FootballApiException s_FootballApiException = new();
    
    private async Task DoWork()
    {
        foreach (var entry in FootballGames)
        {
            if (entry.Value <= 0)
            {
                FootballGames.Remove(entry.Key, out var _);
                continue;
            }
            
            FootballGames.TryUpdate(entry.Key, entry.Value - 1, entry.Value);
            
            await gameDataUpdateQueue.QueueBackgroundWorkItemAsync(async cancellationToken =>
            {
                var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                
                var result = await footballApi.GetGameDetails(entry.Key);
                if (result.IsFaulted)
                    return s_FootballApiException;
                
                var entity = await db.Games
                    .FirstOrDefaultAsync(
                        g => g.ReferenceId == entry.Key && g.SportType == ESportType.Football, 
                        cancellationToken: cancellationToken);

                if (entity != null)
                {
                    // todo update
                    await db.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    var league = result.Value.GameEntry.League;
                    var leaguesById = await db.BuiltinSeasons
                        .Where(s => s.ReferenceId == league.Id)
                        .ToListAsync(cancellationToken);

                    var leagueEntity = leaguesById.MaxBy(s => s.Start);
                    
                    if (leagueEntity is null)
                        return s_FootballApiException;
                        
                    await db.Games.AddAsync(new Game(
                        $"{result.Value.GameEntry.Score.HomeTeam.Name} vs. {result.Value.GameEntry.Score.AwayTeam.Name}", 
                        leagueEntity!.Id)
                    {
                        SportType = ESportType.Football,
                        ReferenceId = entry.Key,
                        StartTime = result.Value.GameEntry.StartTime,
                        GameState = result.Value.GameEntry.Score.State switch
                        {
                            FootballApi.GameState.Cancelled => EGameState.Cancelled,
                            FootballApi.GameState.None => EGameState.Upcoming,
                            FootballApi.GameState.FirstHalf => EGameState.Live,
                            FootballApi.GameState.HalfTimeBreak => EGameState.Live,
                            FootballApi.GameState.SecondHalf => EGameState.Live,
                            FootballApi.GameState.EndedAfterSecondHalf => EGameState.Finished,
                            FootballApi.GameState.BreakAfterSecondHalf => EGameState.Live,
                            FootballApi.GameState.FirstOvertime => EGameState.Live,
                            FootballApi.GameState.OvertimeBreak => EGameState.Live,
                            FootballApi.GameState.SecondOvertime => EGameState.Live,
                            FootballApi.GameState.EndedAfterOverTime => EGameState.Finished,
                            FootballApi.GameState.PenaltyShootout => EGameState.Live,
                            FootballApi.GameState.EndedAfterPenaltyShootout => EGameState.Finished,
                            _ => throw new UnreachableException()
                        }
                    }, cancellationToken);

                    await db.SaveChangesAsync(cancellationToken);
                }
                
                return result.Value;
            });
        }
    }
}