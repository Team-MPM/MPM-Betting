using MPM_Betting.DataModel.Betting;

namespace MPM_Betting.DataModel.Football;

public class GameBet() : Bet(EBetType.FootballGame)
{
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }

    public bool ResultHit { get; set; }
    public bool ScoreHit { get; set; }

    public EResult Result =>
        HomeScore > AwayScore
            ? EResult.Win
            : HomeScore < AwayScore
                ? EResult.Loss
                : EResult.Draw;

    public double Quote { get; set; }
}