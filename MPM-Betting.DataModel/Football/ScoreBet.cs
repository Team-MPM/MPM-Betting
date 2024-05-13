using MPM_Betting.DataModel.Betting;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.DataModel.Football;

public class ScoreBet(MpmUser user, MpmGroup? group, Game game, int homeScore, int awayScore) : Bet(user, group, game, EBetType.FootballResult)
{
    public int HomeScore { get; set; } = homeScore;
    public int AwayScore { get; set; } = awayScore;
    
    private ScoreBet() : this(null!, null!, null!, 0, 0) {}
}