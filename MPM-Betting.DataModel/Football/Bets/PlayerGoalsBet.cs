using System.ComponentModel.DataAnnotations.Schema;
using MPM_Betting.DataModel.Betting;

namespace MPM_Betting.DataModel.Football.Bets;

[Table("FootballPlayerGoalsBet")]
public class PlayerGoalsBet(Competition competition, Fixture fixture, Player player, int goals) 
    : Bet(BetType.PlayerGoals, competition)
{
    public Fixture Fixture { get; set; } = fixture;
    public Player Player { get; set; } = player;
    public int Goals { get; set; } = goals;

    private PlayerGoalsBet() : this(null!, null!, null!, 0)
    {
        
    }
}