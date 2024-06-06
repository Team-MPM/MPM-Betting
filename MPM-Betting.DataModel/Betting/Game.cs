using System.ComponentModel.DataAnnotations;
using MPM_Betting.DataModel.Football;

namespace MPM_Betting.DataModel.Betting;

public class Game(string name, int builtinSeasonId)
{
    [Key]
    public int Id { get; set; }
    
    [StringLength(50)]
    public string Name { get; set; } = name;

    public ESportType SportType { get; set; }

    public List<Bet> Bets { get; set; } = [];
    
    public BuiltinSeason? BuiltinSeason { get; set; }
    public int BuiltinSeasonId { get; set; } = builtinSeasonId;

    public int ReferenceId { get; set; }
    public DateTime StartTime { get; set; }
    public EGameState GameState { get; set; }

    private Game() : this(null!, 0) {}
}