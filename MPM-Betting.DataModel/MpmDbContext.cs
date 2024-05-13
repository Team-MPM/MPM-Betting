using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MPM_Betting.DataModel.Betting;
using MPM_Betting.DataModel.Rewarding;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.DataModel;

public class MpmDbContext(DbContextOptions<MpmDbContext> options) : IdentityDbContext<MpmUser>(options)
{
    public DbSet<MpmGroup> Groups { get; set; } = null!;
    public DbSet<UserGroupEntry> UserGroupEntries { get; set; } = null!;
    public DbSet<SeasonEntry> SeasonEntries { get; set; } = null!;
    public DbSet<Season> Seasons { get; set; } = null!;
    public DbSet<ScoreEntry> ScoreEntries { get; set; } = null!;
    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<CustomSeason> CustomSeasons { get; set; } = null!;
    public DbSet<BuiltinSeason> BuiltinSeasons { get; set; } = null!;
    public DbSet<CustomSeasonEntry> CustomSeasonEntries { get; set; } = null!;
    public DbSet<Bet> Bets { get; set; } = null!;
    public DbSet<Achievement> Achievements { get; set; }
    public DbSet<AchievementEntry> AchievementEntries { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MpmGroup>(builder =>
        {
            builder.HasOne(g => g.CurrentSeason).WithMany();
            builder.HasMany(g => g.Seasons).WithOne(s => s.Group);
            
            builder
                .HasMany(g => g.Seasons)
                .WithOne(se => se.Group)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ScoreEntry>(builder =>
        {
            builder
                .HasOne(se => se.UserGroupEntry)
                .WithMany(uge => uge.ScoreEntries)
                .OnDelete(DeleteBehavior.NoAction);
            
            builder
                .HasOne(se => se.SeasonEntry)
                .WithMany(uge => uge.ScoreEntries)
                .OnDelete(DeleteBehavior.NoAction);
        });
   
        
        base.OnModelCreating(modelBuilder);
    }
}