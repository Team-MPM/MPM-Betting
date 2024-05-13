using System.ComponentModel.DataAnnotations;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.DataModel.Betting;

public class Bet(MpmUser user, MpmGroup? group, Game game, EBetType type)
{
    [Key]
    public int Id { get; set; }

    public MpmUser User { get; set; } = user;
    public MpmGroup? Group { get; set; } = group;

    public bool Completed { get; set; }
    
    public int Score { get; set; }
    
    public Game Game { get; set; } = game;

    public EBetType Type { get; set; } = type;
    
    private Bet() : this(null!, null!, null!, EBetType.None) {}
}