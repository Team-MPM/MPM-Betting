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
    public DbSet<Achievement> Achievements { get; set; }
    public DbSet<AchievementEntry> AchievementEntries { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<FavoriteSeasons> FavoriteSeasons { get; set; }
    public DbSet<UserHasFouvoriteSeasons> UserFavouriteSeasons { get; set; }
    
    public DbSet<Bet> Bets { get; set; } = null!;
    public DbSet<Football.ResultBet> FootballResultBets { get; set; } = null!;
    public DbSet<Football.ScoreBet> FootballScoreBets { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FavoriteSeasons>(builder =>
        {
           builder.HasKey(fs => new {fs.UserId, fs.SeasonId});
        });

        modelBuilder.Entity<UserHasFouvoriteSeasons>()
            .HasKey(s =>  new { s.UserId, s.LeaueeId });

        modelBuilder.Entity<UserHasFouvoriteSeasons>()
            .HasOne<MpmUser>(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId);
        
        modelBuilder.Entity<Bet>(builder =>
        {
            builder.ToTable(nameof(Bets));
        });
        
        modelBuilder.Entity<Football.ResultBet>(builder =>
        {
            builder.ToTable(nameof(FootballResultBets));
            builder.HasBaseType<Bet>();
        });
        
        modelBuilder.Entity<Football.ScoreBet>(builder =>
        {
            builder.ToTable(nameof(FootballScoreBets));
            builder.HasBaseType<Bet>();
        });
        
        modelBuilder.Entity<MpmGroup>(builder =>
        {
            builder.HasMany(g => g.Seasons).WithOne(s => s.Group);
            
            builder
                .HasMany(g => g.Seasons)
                .WithOne(se => se.Group)
                .OnDelete(DeleteBehavior.NoAction);
            
            builder
                .HasMany(g => g.UserGroupEntries)
                .WithOne(uge => uge.Group)
                .OnDelete(DeleteBehavior.NoAction);
        });
        
        modelBuilder.Entity<Bet>(builder =>
        {
            builder
                .HasOne(b => b.Group)
                .WithMany(g => g.AllBets)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<CustomSeasonEntry>(builder =>
        {
            builder
                .HasOne(cse => cse.Season)
                .WithMany(s => s.Entries)
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