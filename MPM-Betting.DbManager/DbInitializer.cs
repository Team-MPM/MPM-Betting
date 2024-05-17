﻿using System.Collections.Concurrent;
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

internal class DbInitializer(IWebHostEnvironment env, IServiceProvider serviceProvider, ILogger<DbInitializer> logger, FootballApi footballApi)
    : BackgroundService
{
    public const string ActivitySourceName = "Migrations";

    private readonly ActivitySource m_ActivitySource = new(ActivitySourceName);

    private UserDomain m_UserDomain = null!;
    private MpmDbContext m_DbContext = null!;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        m_DbContext = scope.ServiceProvider.GetRequiredService<MpmDbContext>();
        m_UserDomain = scope.ServiceProvider.GetRequiredService<UserDomain>();
        await InitializeDatabaseAsync(cancellationToken);
    }

    private async Task InitializeDatabaseAsync(CancellationToken cancellationToken)
    {
        using var activity = m_ActivitySource.StartActivity(ActivityKind.Client);

        var sw = Stopwatch.StartNew();

        var strategy = m_DbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(m_DbContext.Database.MigrateAsync, cancellationToken);

        await SeedAsync(cancellationToken);

        logger.LogInformation("Database initialization completed after {ElapsedMilliseconds}ms",
            sw.ElapsedMilliseconds);
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Seeding database");

        await SeedBuiltinSeasons(cancellationToken);
        //await SeedAchievements();

        if (env.IsDevelopment())
        {
            await SeedTestGroups();
        }

        await m_DbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedAchievements()
    {
        var achievements = new List<MpmResult<Achievement>>
        {
            await m_UserDomain.CreateAchievement("First Steps", "Place your first bet"),
            await m_UserDomain.CreateAchievement("Victory Royale", "Win your first bet"),
            await m_UserDomain.CreateAchievement("Womp Womp", "Lose your first bet"),
            await m_UserDomain.CreateAchievement("Getting Started", "Place 10 Bets"),
            await m_UserDomain.CreateAchievement("High Roller", "Place 100 Bets")
        };

        //TODO: Insert achievements in db
    }
    private async Task SeedTestGroups()
    {
        if(m_DbContext.Groups.Count() < 200)
           return; 

        var testGroups = new List<MpmResult<MpmGroup>>();

        for (int i = 0; i < 300; i++)
        {
            testGroups.Add(await m_UserDomain.CreateGroup("Test"+i.ToString(), "Test Group"+ i.ToString()));
        }

        var testCustomSeasons = new List<MpmResult<CustomSeason>>();

        for (int i = 0; i < 100; i++)
        {
            testCustomSeasons.Add(await m_UserDomain.CreateCustomSeason(testGroups[i].Value,$"Mixed League{i}", $"Mixed League{i}", new DateTime(2024, 9, 1), new DateTime(2025, 6, 31)));             
        }
        
        // TODO: Julian: add games to custom seasons
            
        for (int i = 101; i < testGroups.Count-1; i++)
        {
            var currentBuiltInSeasonById = await m_UserDomain.GetCurrentBuiltInSeasonById(87);
            if (currentBuiltInSeasonById.IsSuccess)
                await m_UserDomain.AddSeasonToGroup(testGroups[i].Value, currentBuiltInSeasonById.Value);
        }
    }

    private async Task SeedBuiltinSeasons(CancellationToken cancellationToken)
    {
        if (m_DbContext.BuiltinSeasons.Count() > 2000)
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

        await m_DbContext.BuiltinSeasons.AddRangeAsync(allSeasons, cancellationToken);
    }
}