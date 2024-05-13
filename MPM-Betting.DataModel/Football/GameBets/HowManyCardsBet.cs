using System.ComponentModel.DataAnnotations.Schema;
using MPM_Betting.DataModel.Betting;

namespace MPM_Betting.DataModel.Football;

public class HowManyCardsBet : Bet
{
    [NotMapped]
    public int GuessedCardCount => n1;
    [NotMapped]
    public int RealCardCount => n2;
}