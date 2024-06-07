using MPM_Betting.DataModel.Betting;

namespace MPM_Betting.DataModel.Football;

public class ScoreBet: Bet
{
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    
    public int Points { get; set; }
    
    public int MatchId { get; set; }
    
    public double Quote { get; set; }

    public ScoreBet(int homeScore, int awayScore)
    {
        Type = EBetType.FootballScore;
        HomeScore = homeScore;
        AwayScore = awayScore;
    }
}