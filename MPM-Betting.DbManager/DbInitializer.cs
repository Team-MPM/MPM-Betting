using System.Diagnostics;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using MPM_Betting.DataModel;
using MPM_Betting.DataModel.Betting;

namespace MPM_Betting.DbManager;

internal class DbInitializer(IServiceProvider serviceProvider, ILogger<DbInitializer> logger)
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
        if (await dbContext.Seasons.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Database already seeded");
            return;
        }

        logger.LogInformation("Seeding database");

        var builtinSeasons = new[]
        {
            new BuiltinSeason("UEFA Champions League", "Top competition in Europe")
            {
                Sport = ESportType.Football,
                Start = DateTime.ParseExact("9.7.2023", "d.M.yyyy", CultureInfo.InvariantCulture),
                End = DateTime.ParseExact("31.5.2024", "d.M.yyyy", CultureInfo.InvariantCulture),
                ReferenceId = 42
            }
        };

        await dbContext.Seasons.AddRangeAsync(builtinSeasons, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}