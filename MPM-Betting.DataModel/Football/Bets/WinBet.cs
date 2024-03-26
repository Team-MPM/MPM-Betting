using System.ComponentModel.DataAnnotations.Schema;
using MPM_Betting.DataModel.Betting;

namespace MPM_Betting.DataModel.Football.Bets;

[Table("FootballWinBet")]
public class WinBet(Competition competition, Fixture fixture, Team? team) 
    : Bet(BetType.Win, competition)
{
    public Fixture Fixture { get; set; } = fixture;
    
    /// <summary>
    /// Null means draw
    /// </summary>
    public Team? Team { get; set; } = team;

    private WinBet() : this(null!, null!, null)
    {
        
    }
}