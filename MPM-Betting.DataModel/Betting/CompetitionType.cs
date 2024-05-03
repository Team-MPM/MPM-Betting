using System.ComponentModel.DataAnnotations;

namespace MPM_Betting.DataModel.Betting;

public abstract class ACompetitionType(int ID, string Name)
{
    [Key]
    public int ID { get; set; }
    public string Name { get; set; }
}