using System.Net.Mail;
using LanguageExt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MPM_Betting.DataModel;
using MPM_Betting.DataModel.User;
using MPM_Betting.Services.Account;
using MPM_Betting.Services.Data;
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
    
    public static IHostApplicationBuilder AddMpmDbContext(this IHostApplicationBuilder builder)
    {
        builder.AddSqlServerDbContext<MpmDbContext>("MPM-Betting");
        
        builder.Services.AddDataProtection()
            .SetApplicationName("Mpm-Betting");

        builder.Services.AddScoped<UserDomain>();
        
        return builder;
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
        builder.Services.AddSingleton<SmtpClient>((sp) =>
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
        Func<Exception, IResult> defaultErrorHandler = err => status500;
        
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
        
        return app;
    }
}
