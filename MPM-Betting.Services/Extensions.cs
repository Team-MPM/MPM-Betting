using System.Net.Mail;
using System.Runtime.CompilerServices;
using Aspire.Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MPM_Betting.DataModel;
using MPM_Betting.DataModel.Betting;
using MPM_Betting.DataModel.User;
using MPM_Betting.Services.Account;
using MPM_Betting.Services.Data;
using MPM_Betting.Services.Data.GameData;
using MPM_Betting.Services.Domains;

namespace MPM_Betting.Services;

public static class Extensions
{
    public static WebApplicationBuilder AddFootballApi(this  WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<FootballApi>();
        return builder;
    }
    
    
    public static WebApplicationBuilder AddMpmCache(this  WebApplicationBuilder builder)
    {
        builder.Services.AddMemoryCache();
        builder.AddRedisDistributedCache("redis");
        builder.AddRedisOutputCache("redis");
        builder.Services.AddSingleton<MpmCache>();
        return builder;
    }

    public static IHostApplicationBuilder AddDomainLayer(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpClient<UserDomain>(client => client.BaseAddress = new("http://api"));
        return builder;
    }

    private static void EnsureDbContextNotRegistered<TContext>(this IHostApplicationBuilder builder, [CallerMemberName] string callerMemberName = "") where TContext : DbContext
    {
        if (!builder.Environment.IsDevelopment())
        {
            return;
        }

        var oldDbContextOptionsDescriptor = builder.Services.FirstOrDefault(sd => sd.ServiceType == typeof(DbContextOptions<TContext>));

        if (oldDbContextOptionsDescriptor is not null)
        {
            throw new InvalidOperationException($"DbContext<{typeof(TContext).Name}> is already registered. Please ensure 'services.AddDbContext<{typeof(TContext).Name}>()' is not used when calling '{callerMemberName}()' or use the corresponding 'Enrich' method.");
        }
    }

    private static TSettings GetDbContextSettings<TContext, TSettings>(this IHostApplicationBuilder builder, string defaultConfigSectionName, Action<TSettings, IConfiguration> bindSettings)
        where TSettings : new()
    {
        TSettings settings = new();
        var typeSpecificSectionName = $"{defaultConfigSectionName}:{typeof(TContext).Name}";
        var typeSpecificConfigurationSection = builder.Configuration.GetSection(typeSpecificSectionName);
        if (typeSpecificConfigurationSection.Exists()) // https://github.com/dotnet/runtime/issues/91380
        {
            bindSettings(settings, typeSpecificConfigurationSection);
        }
        else
        {
            var section = builder.Configuration.GetSection(defaultConfigSectionName);
            bindSettings(settings, section);
        }

        return settings;
    }
    
    private const string DefaultConfigSectionName = "Aspire:Microsoft:EntityFrameworkCore:SqlServer";
    
 
    
    public static IHostApplicationBuilder AddMpmDbContext(this IHostApplicationBuilder builder)
    {
        //builder.AddSqlServerDbContext<MpmDbContext>("MPM-Betting");
        
        
        builder.EnsureDbContextNotRegistered<MpmDbContext>();

        var settings = builder.GetDbContextSettings<MpmDbContext, MicrosoftEntityFrameworkCoreSqlServerSettings>(
            DefaultConfigSectionName,
            (settings, section) => section.Bind(settings)
        );

        if (builder.Configuration.GetConnectionString("MPM-Betting") is { } connectionString)
        {
            settings.ConnectionString = connectionString;
        }

        builder.Services.AddDbContextPool<MpmDbContext>(ConfigureDbContext);
        builder.Services.AddDbContextFactory<MpmDbContext>(ConfigureDbContext);

        builder.Services.AddDataProtection()
            .SetApplicationName("Mpm-Betting");
        
        builder.Services.AddHttpClient<UserDomain>(client => client.BaseAddress = new("http://api"));
        
        return builder;

        void ConfigureDbContext(DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            dbContextOptionsBuilder.UseSqlServer(settings.ConnectionString, builder =>
            {
                if (!settings.DisableRetry)
                {
                    builder.EnableRetryOnFailure();
                }

                if (settings.CommandTimeout.HasValue)
                {
                    builder.CommandTimeout(settings.CommandTimeout);
                }
            });
        }
    }
    
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    public static WebApplicationBuilder AddMpmAuth(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "placeholder";
                googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "placeholder";
            })
            .AddMicrosoftAccount(microsoftOptions =>
            {
                microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"] ?? "placeholder";
                microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"] ?? "placeholder";
            })
            .AddCookie("Identity.Application")
            .AddCookie("Identity.External");
        
        builder.Services.AddAuthorization();
        
        builder.Services.AddIdentityCore<MpmUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddEntityFrameworkStores<MpmDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        builder.Services.AddSingleton<IEmailSender<MpmUser>, IdentityEmailSender>();

        return builder;
    }

    public static WebApplicationBuilder AddMpmMail(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<SmtpClient>(_ =>
        {
            var smtpUri = new Uri(builder.Configuration.GetConnectionString("maildev")!);
            var smtpClient = new SmtpClient(smtpUri.Host, smtpUri.Port);
            return smtpClient;
        });

        return builder;
    }
    
    public static WebApplication MapFootballEndpoints(this WebApplication app)
    {
        // TODO: separate leagues and cups
        var status500 = Results.Problem("An internal server error occurred", statusCode: 500);
        Func<Exception, IResult> defaultErrorHandler = _ => status500;
        
        app.MapGet("/football/leagues", async ([FromServices] FootballApi api) 
                    => (await api.GetAllFootballLeagues()).Match(Results.Ok, defaultErrorHandler))
            .WithName("GetAllFootballLeagues")
            .WithOpenApi();
        
        app.MapGet("/football/teams", 
                async ([FromServices] FootballApi api, [FromQuery] int? league) => league is null 
                        ? (await api.GetAllFootballTeams()).Match(Results.Ok, defaultErrorHandler)
                        : (await api.GetTeamsFromLeague(league.Value)).Match(Results.Ok, defaultErrorHandler))
            .WithName("GetAllFootballTeams")
            .WithOpenApi();
        
        app.MapGet("/football/players", 
                async ([FromServices] FootballApi api, [FromQuery] int? team) => team is null 
                    ? (await api.GetAllFootballPlayers()).Match(Results.Ok, defaultErrorHandler)
                    : (await api.GetPlayersFromTeam(team.Value)).Match(Results.Ok, defaultErrorHandler))
            .WithName("GetAllFootballPlayers")
            .WithOpenApi();
        
        app.MapGet("/football/table", 
                async ([FromServices] FootballApi api, [FromQuery] int league, [FromQuery] string? season) 
                    => (await api.GetLeagueTable(league, season)).Match(Results.Ok, err => err switch
                    {
                        FootballApi.LeagueNotFoundException ex => Results.NotFound(ex.Message),
                        _ => status500
                    }))
            .WithName("GetTable")
            .WithOpenApi();

        app.MapGet("/football/games",
            async ([FromServices] FootballApi api, [FromQuery] int league, [FromQuery] DateOnly? date) 
                => (await api.GetGameEntries(league, date)).Match(Results.Ok, err => err switch
                {
                    FootballApi.LeagueNotFoundException ex => Results.NotFound(ex.Message),
                    _ => status500
                }))
            .WithName("GetGames")
            .WithOpenApi();

        app.MapGet("/football/gameDetails", async ([FromServices] FootballApi api, [FromQuery] int gameId) 
                => (await api.GetGameDetails(gameId)).Match(Results.Ok, err => err switch
                {
                    FootballApi.GameNotFoundException ex => Results.NotFound(ex.Message),
                    _ => status500
                }))
            .WithName("GetGameDetails")
            .WithOpenApi();
        
        app.MapGet("/football/track/league/{leagueId:int}", (
                int leagueId, 
                [FromServices] GameDataUpdateScheduler gdus) =>
            {
                if (gdus.FootballLeagues.TryGetValue(leagueId, out var x))
                {
                    gdus.FootballLeagues.TryUpdate(leagueId, 100, x);
                    return StatusCodes.Status302Found;
                }
                else
                {
                    gdus.FootballLeagues.TryAdd(leagueId, 100);
                    return StatusCodes.Status201Created;
                }
            })
            .WithName("TrackLeague")
            .WithOpenApi();
        
        
        app.MapGet("/football/track/game/{gameId:int}", async (
                int gameId, 
                [FromServices] GameDataUpdateScheduler gdus,
                [FromServices] IDbContextFactory<MpmDbContext> dbContextFactory) =>
            {
                var context = await dbContextFactory.CreateDbContextAsync();
                var game = await context.Games.FirstOrDefaultAsync(g => g.ReferenceId == gameId);
                if (gdus.FootballGames.TryGetValue(gameId, out var x))
                {
                    gdus.FootballGames.TryUpdate(gameId, 100, x);
                    return game is null ? Results.StatusCode(StatusCodes.Status102Processing) : Results.StatusCode(StatusCodes.Status302Found);
                }

                gdus.FootballGames.TryAdd(gameId, 100);
                return Results.StatusCode(StatusCodes.Status201Created);
                //return StatusCodes.Status201Created;
            })
            .WithName("TrackGame")
            .WithOpenApi();
        
        return app;
    }
}
