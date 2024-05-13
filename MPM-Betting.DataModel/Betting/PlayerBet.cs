using System.ComponentModel.DataAnnotations.Schema;
using MPM_Betting.DataModel.Betting;

namespace MPM_Betting.DataModel.Football.GameBets;
[NotMapped]
public class PlayerBet :Bet
{
    public int PlayerId { get; set; }
    
}