using System.ComponentModel.DataAnnotations;

namespace MPM_Betting.DataModel.Football;

public class Fixture(Team t1, Team t2, DateTime date)
{
    [Key] public int Id { get; set; }
    public Team Team1 { get; set; } = t1; // Home
    public Team Team2 { get; set; } = t2; // Away
    public DateTime Date { get; set; } = date;

    private Fixture() : this(null!, null!, DateTime.Today)
    {
        
    }
}