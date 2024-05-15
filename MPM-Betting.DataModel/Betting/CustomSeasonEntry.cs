using System.ComponentModel.DataAnnotations;

namespace MPM_Betting.DataModel.Betting;

public class CustomSeasonEntry(CustomSeason season, Game game)
{
    [Key] 
    public int Id { get; set; }

    public CustomSeason Season { get; set; } = season;

    public Game Game { get; set; } = game;
    
    private CustomSeasonEntry() : this(null!, null!) { }
}