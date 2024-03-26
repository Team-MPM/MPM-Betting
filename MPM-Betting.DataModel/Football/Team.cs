using System.ComponentModel.DataAnnotations;
using MPM_Betting.DataModel.Betting;

namespace MPM_Betting.DataModel.Football;

public class Team
{
    [Key] public int Id { get; set; }
    [StringLength(30)] public string Name { get; set; } = "";
    public List<Competition> Competitions { get; set; } = [];
    public List<Player> Players { get; set; } = [];
    public List<Fixture> HomeFixtures { get; set; } = [];
    public List<Fixture> AwayFixtures { get; set; } = [];
}