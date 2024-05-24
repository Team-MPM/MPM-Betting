using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MPM_Betting.DataModel;
using MPM_Betting.DataModel.Betting;
using MPM_Betting.DataModel.Rewarding;
using MPM_Betting.DataModel.User;
using MPM_Betting.Services;
using MPM_Betting.Services.Data;
using MPM_Betting.Services.Domains;
using StackExchange.Redis;

namespace MPM_Betting.DbManager;

internal class DbInitializer(UserDomain userDomain, IWebHostEnvironment env, IServiceProvider serviceProvider, ILogger<DbInitializer> logger, FootballApi footballApi)
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

        if (env.IsDevelopment())
        {
            await SeedTestGoups(dbContext);
        }

        //await SeedAchievments(dbContext);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedAchievments(MpmDbContext dbContext)
    {
        List<MpmResult<Achievement>> achievements = new List<MpmResult<Achievement>>();
        
        achievements.Add(await userDomain.CreateAchievement("First Steps", "Place your first bet"));
        achievements.Add(await userDomain.CreateAchievement("Victory Royale", "Win your first bet"));
        achievements.Add(await userDomain.CreateAchievement("Womp Womp", "Lose your first bet"));
        achievements.Add(await userDomain.CreateAchievement("Getting Started", "Place 10 Bets"));
        achievements.Add(await userDomain.CreateAchievement("High Roller", "Place 100 Bets"));
        
        //TODO: Insert achiements in db
    }
    private async Task SeedTestGoups(MpmDbContext dbContext)
    {
        if(dbContext.Groups.Count() < 200)
           return; 

        List<MpmResult<MpmGroup>> TestGroups = new List<MpmResult<MpmGroup>>();

        for (int i = 0; i < 300; i++)
        {
            TestGroups.Add(await userDomain.CreateGroup("Test"+i.ToString(), "Test Group"+ i.ToString()));
        }

        List<MpmResult<CustomSeason>> testCustomSeasons = new List<MpmResult<CustomSeason>>();

        for (int i = 0; i < 100; i++)
        {
            testCustomSeasons.Add(await userDomain.CreateCustomSeason(TestGroups[i].Value,$"Mixed League{i}", $"Mixed League{i}", new DateTime(2024, 9, 1), new DateTime(2025, 6, 31)));             
        }
            
        for (int i = 101; i < TestGroups.Count-1; i++)
        {
            var curentBuiltInSeason = await userDomain.GetCurrentBuiltInSeasonById(i);
            await userDomain.AddSeasonToGroup(TestGroups[i].Value, curentBuiltInSeason.Value);
        }
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
                        Sport = ESportType.Football,
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
