using System.ComponentModel.DataAnnotations;

namespace MPM_Betting.DataModel.Betting;

public class Game(string name, BuiltinSeason season)
{
    [Key]
    public int Id { get; set; }
    
    public string Name { get; set; } = name;

    public ESportType SportType { get; set; }

    public List<Bet> Bets { get; set; }
    
    public BuiltinSeason? BuiltinSeason { get; set; } = season;

    public int ReferenceId { get; set; }

    private Game() : this(null!, null!) {}
}