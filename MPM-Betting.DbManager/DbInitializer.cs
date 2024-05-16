using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using MPM_Betting.DataModel;
using MPM_Betting.DataModel.Betting;
using MPM_Betting.Services.Data;
using StackExchange.Redis;

namespace MPM_Betting.DbManager;

internal class DbInitializer(IServiceProvider serviceProvider, ILogger<DbInitializer> logger, FootballApi footballApi)
    : BackgroundService
{
    public const string ActivitySourceName = "Migrations";

    private readonly ActivitySource m_ActivitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MpmDbContext>();

        await InitializeDatabaseAsync(dbContext, cancellationToken);
    }

    private async Task InitializeDatabaseAsync(MpmDbContext dbContext, CancellationToken cancellationToken)
    {
        using var activity = m_ActivitySource.StartActivity(ActivityKind.Client);

        var sw = Stopwatch.StartNew();

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(dbContext.Database.MigrateAsync, cancellationToken);

        await SeedAsync(dbContext, cancellationToken);

        logger.LogInformation("Database initialization completed after {ElapsedMilliseconds}ms",
            sw.ElapsedMilliseconds);
    }

    private async Task SeedAsync(MpmDbContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogInformation("Seeding database");

        await SeedBuiltinSeasons(dbContext, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedBuiltinSeasons(MpmDbContext dbContext, CancellationToken cancellationToken)
    {
        if (dbContext.BuiltinSeasons.Count() > 2000)
            return;
        
        var result = await footballApi.GetAllFootballLeagues();
        var allSeasons = new ConcurrentBag<BuiltinSeason>();

        if (result.IsFaulted)
            return;

        Parallel.ForEach(result.Value, league =>
        {
            try
            {
                var seasonResult = footballApi.GetSeasonsForLeague(league.Id);
                seasonResult.Wait(cancellationToken);
                seasonResult.Result.IfSuc(seasons =>
                {
                    foreach (var season in seasons.Select(s => new BuiltinSeason(league.Name, league.Name)
                    {
                        ReferenceId = league.Id,
                        Start = new DateTime(int.Parse(s.Split('/')[0]), 1, 1),
                        End = new DateTime(int.Parse(s.Split('/')[1]), 1, 1)
                    })) allSeasons.Add(season);
                });
            }
            catch (Exception e)
            {
                logger.LogInformation("SeedBuiltinSeasons encountered {Message} at league {LeagueId}", e.Message, league.Id);
            }
        });

        await dbContext.BuiltinSeasons.AddRangeAsync(allSeasons, cancellationToken);
    }
}