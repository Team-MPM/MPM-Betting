using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MPM_Betting.DataModel.User;


public class Bet
{   
    [Column("ID")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }
    
    [Column("GROUP_ID")]
    public int GROUP_ID { get; set; }
    public MpmGroup Group { get; set; }
    
    public List<MpmUser> Users { get; set; }
    [Column("MATCH_ID")]
    public int MatchID { get; set; }

    public EBetType BetType { get; set; }
    public EBetTargetType BetTargetType { get; set; }
}