using System.ComponentModel.DataAnnotations;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.DataModel.Betting;

public class SeasonEntry(string name, MpmGroup group)
{
    [Key]
    public int Id { get; set; }

    public MpmGroup Group { get; set; } = group;

    public Season Season { get; set; }

    private SeasonEntry() : this(null!, null!) { }
}