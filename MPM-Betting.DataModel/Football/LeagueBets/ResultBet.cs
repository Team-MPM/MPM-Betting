using MPM_Betting.DataModel.Betting;

namespace MPM_Betting.DataModel.Football.LeagueBets;

public class ResultBet : LeagueBet
{
    public int GuessedWinnerId => n1;
    public int RealWinnerId => n2;
}