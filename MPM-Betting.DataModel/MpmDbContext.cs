using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MPM_Betting.DataModel.Betting;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.DataModel;

public class MpmDbContext(DbContextOptions<MpmDbContext> options) : IdentityDbContext<MpmUser>(options)
{
    public DbSet<MpmGroup> Groups { get; set; } = null!;
    public DbSet<UserGroupEntry> UserGroupEntries { get; set; } = null!;
    public DbSet<Competition> Competitions { get; set; } = null!;

    public DbSet<Football.Fixture> FootballFixtures { get; set; } = null!;
    public DbSet<Football.Team> FootballTeams { get; set; } = null!;
    public DbSet<Football.Player> FootballPlayers { get; set; } = null!;
    
    public DbSet<Bet> Bets { get; set; } = null!;
    public DbSet<Football.Bets.WinBet> FootballWinBets { get; set; } = null!;
    public DbSet<Football.Bets.ScoreBet> FootballScoreBets { get; set; } = null!;
    public DbSet<Football.Bets.PlayerGoalsBet> FootballPlayerGoalsBets { get; set; } = null!;
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MPM-Betting;Trusted_Connection=True;MultipleActiveResultSets=true");
        //optionsBuilder.UseSqlite("Data Source=data.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Football.Fixture>()
            .HasOne(f => f.Team1)
            .WithMany(t => t.HomeFixtures)
            .OnDelete(DeleteBehavior.NoAction); 

        modelBuilder.Entity<Football.Fixture>()
            .HasOne(f => f.Team2)
            .WithMany(t => t.AwayFixtures)
            .OnDelete(DeleteBehavior.Cascade);
        
        base.OnModelCreating(modelBuilder);
    }
}