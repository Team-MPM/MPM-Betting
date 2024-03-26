using System.ComponentModel.DataAnnotations;

namespace MPM_Betting.DataModel.Betting;

public abstract class Bet(BetType type, Competition competition)
{
    [Key]
    public int Id { get; set; }
    public BetType Type { get; set; } = type;
    public Sport Sport { get; set; } = competition.Sport; // TODO: Check if this works with ef initializer mappings
    public Competition Competition { get; set; } = competition;
}