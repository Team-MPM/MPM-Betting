using System.ComponentModel.DataAnnotations;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.DataModel.Betting;

public class SeasonEntry
{
    [Key]
    public int Id { get; set; }

    public int GroupId { get; set; }
    public MpmGroup Group { get; set; }

    public int SeasonId { get; set; }
    public Season Season { get; set; }

    public bool IsActive { get; set; } = true;

    public List<ScoreEntry> ScoreEntries { get; set; } = [];
}