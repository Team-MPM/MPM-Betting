using System.ComponentModel.DataAnnotations.Schema;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.DataModel.Betting;
[NotMapped]
public class LeagueBet
{
    public int LeagueBetId { get;set; }
    public int CompetitionId { get;set; }
    public User.MpmUser User { get;set; }
    
    public int n1 { get;set; }
    public int n2 { get;set; }
    
}