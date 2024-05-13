using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MPM_Betting.DataModel.Football;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.DataModel;

public class MpmDbContext(DbContextOptions<MpmDbContext> options) : IdentityDbContext<MpmUser>(options)
{
    public DbSet<MpmGroup> Groups { get; set; } = null!;
    public DbSet<UserGroupEntry> UserGroupEntries { get; set; } = null!;
    public DbSet<Betting.Bet> Bets { get; set; } = null!;
    public DbSet<Football.ResultBet> F_ResultBets { get; set; } = null!;
    public DbSet<Football.ScoreBet> F_ScoreBets { get; set; } = null!;
    public DbSet<WhichBodyPartBet> F_WhichBodyPartBets { get; set; } = null!;
    public DbSet<HowManyCardsBet> F_HowManyCardsBets { get; set; } = null!;
    public DbSet<GoalDistanceBet> F_GoalDistanceBet { get; set; } = null!;
    public DbSet<WhichSideOfGoalBet> F_WhichSideOfGoalBet { get; set; } = null!;
    
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Betting.Bet>()
            .HasDiscriminator<string>("BetType")
            .HasValue<Football.ResultBet>("FootballResultBet")
            .HasValue<Football.ScoreBet>("FootballScoreBet")
            .HasValue<WhichBodyPartBet>("FootballWhichBodyPartBet")
            .HasValue<HowManyCardsBet>("FootballHowManyCardsBet")
            .HasValue<GoalDistanceBet>("FootballGoalDistanceBet")
            .HasValue<WhichBodyPartBet>("FootballWhichBodyPartBet");

    }
}
//TODO Add other DbSets
// Command für Migration: dotnet ef migrations add --startup-project .\MPM-Betting.DbManager\MPM-Betting.DbManager.csproj init 
// smthing with trophies and archivements