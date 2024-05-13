using System.ComponentModel.DataAnnotations.Schema;
using MPM_Betting.DataModel.Betting;

namespace MPM_Betting.DataModel.Football;

public class WhichSideOfGoalBet : Bet
{   [NotMapped]
    public ESide RealSide
    {
        get
        {
            return (ESide)n2;
        }
        set
        {
            n2 = (int)value;
        }
    }
    [NotMapped]
    public ESide GuessedSide
    {
        get
        {
            return (ESide)n1;
            
        }
        set
        {
            n1 = (int)value;
        }
    }
}