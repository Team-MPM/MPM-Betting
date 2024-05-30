using System.ComponentModel.DataAnnotations;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.DataModel.Betting;

public class Bet()
{
    [Key]
    public int Id { get; set; }

    public string UserId { get; set; }
    public MpmUser User { get; set; }

    public int? GroupId { get; set; }
    public MpmGroup? Group { get; set; }

    public bool Completed { get; set; }
    
    public int Score { get; set; }

    public int GameId { get; set; }
    public Game Game { get; set; }

    public EBetType Type { get; set; }

    public bool Processed { get; set; } = false;
    public bool Hit { get; set; } = false;
}