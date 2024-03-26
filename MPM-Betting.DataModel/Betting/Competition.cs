using System.ComponentModel.DataAnnotations;

namespace MPM_Betting.DataModel.Betting;

public class Competition
{
    [Key] public int Id { get; set; }
    public Sport Sport { get; set; }
    [StringLength(30)] public string Name { get; set; } = "";
    public Country Country { get; set; }
}