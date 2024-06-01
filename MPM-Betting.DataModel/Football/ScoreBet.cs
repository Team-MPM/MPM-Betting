using MPM_Betting.DataModel.Betting;

namespace MPM_Betting.DataModel.Football;

public class ScoreBet : Bet
{
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }

    public ScoreBet()
    {
        Type = EBetType.FootballScore;
    }
}