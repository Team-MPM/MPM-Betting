using MPM_Betting.DataModel.Betting;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.DataModel.Football;

public class ResultBet(MpmUser user, MpmGroup? group, Game game, EResult result) : Bet(user, group, game, EBetType.FootballResult)
{
    public EResult Result { get; set; } = result;
    
    private ResultBet() : this(null!, null!, null!, EResult.None) {}
}