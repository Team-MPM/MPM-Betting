using Microsoft.EntityFrameworkCore;
using MPM_Betting.DataModel;
using MPM_Betting.DbManager;
using MPM_Betting.Services;
using MPM_Betting.Services.Data.GameData;
using MPM_Betting.Services.Domains;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddMpmCache();
builder.AddFootballApi();

builder.AddSqlServerDbContext<MpmDbContext>("MPM-Betting", null,
    optionsBuilder =>
    {
        optionsBuilder.UseSqlServer(sqlBuilder =>
        {
            sqlBuilder.MigrationsAssembly(typeof(Program).Assembly.GetName().Name);
            sqlBuilder.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });

        optionsBuilder.EnableDetailedErrors();
    });

//builder.Services.AddTransient<UserDomain>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(DbInitializer.ActivitySourceName));

builder.Services.AddSingleton<DbInitializer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DbInitializer>());

builder.Services.AddHealthChecks()
    .AddCheck<DbInitializerHealthCheck>("DbInitializer", null);

var app = builder.Build();

app.MapDefaultEndpoints();

await app.RunAsync();