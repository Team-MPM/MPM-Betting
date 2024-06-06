using MPM_Betting.DataModel.Betting;

namespace MPM_Betting.DataModel.Football;

public class ResultBet : Bet
{
    public EResult Result { get; set; }
    public int QuoteHome { get; set; }
    public int QuoteDraw { get; set; }
    public int QuoteAway { get; set; }

    public ResultBet()
    {
        Type = EBetType.FootballResult;
    }
}