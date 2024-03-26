using System.ComponentModel.DataAnnotations.Schema;
using MPM_Betting.DataModel.Betting;

namespace MPM_Betting.DataModel.Football.Bets;

[Table("FootballScoreBet")]
public class ScoreBet(Competition competition, Fixture fixture, int score1, int score2) 
    : Bet(BetType.Score, competition)
{
    public Fixture Fixture { get; set; } = fixture;
    public int Score1 { get; set; } = score1;
    public int Score2 { get; set; } = score2;

    private ScoreBet() : this(null!, null!, 0, 0)
    {
        
    }
}