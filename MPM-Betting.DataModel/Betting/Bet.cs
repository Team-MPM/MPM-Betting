using System.ComponentModel.DataAnnotations;
using System.Xml;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.DataModel.Betting;
/// <summary>
/// Bet BaseClass
/// </summary>
public class Bet
{
    [Key]
    public int Id { get; set; }
    public int MatchID { get; set; }
    public MpmUser User { get; set; }
    
    //union
    public int n1 { get; set; }
    public int n2 { get; set; }
}
//TODO: Add PlayerBet => prolly neue BaseClass
//TODO: Add some LeagueBets

//TODO: Add GoalDistanceBet + implement
//TODO: Add WhichSideOfGoalBet + implement
//TODO: Add WhichBodyPartBet
//TODO: Add HowManyCardsBet
//TODO: WhichTeamHasMoreShotsOnGoalBet
//TODO: WhichTeamHasMorePossessionBet
//TODO: WhichTeamHasMoreCornersBet
//TODO: HowManyBallsSavedOfLineBet