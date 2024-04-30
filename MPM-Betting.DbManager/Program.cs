using Microsoft.EntityFrameworkCore;
using MPM_Betting.DataModel;
using MPM_Betting.DbManager;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddSqlServerDbContext<MpmDbContext>("MPM-Betting", null,
    optionsBuilder => optionsBuilder.UseSqlServer(sqlBuilder =>
        sqlBuilder.MigrationsAssembly(typeof(Program).Assembly.GetName().Name)));

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(DbInitializer.ActivitySourceName));

builder.Services.AddSingleton<DbInitializer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DbInitializer>());
builder.Services.AddHealthChecks()
    .AddCheck<DbInitializerHealthCheck>("DbInitializer", null);

var app = builder.Build();

app.MapDefaultEndpoints();

await app.RunAsync();