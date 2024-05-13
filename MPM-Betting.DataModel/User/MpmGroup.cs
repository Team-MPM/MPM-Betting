using System.ComponentModel.DataAnnotations;
using MPM_Betting.DataModel.Betting;

namespace MPM_Betting.DataModel.User;

public class MpmGroup
{
    [Key]
    public int Id { get; set; }
    
    public MpmUser Creator { get; set; }
    
    public List<UserGroupEntry> UserGroupEntries { get; set; } = [];
    
    [Required]
    [StringLength(30)]
    public string Name { get; set; }

    [StringLength(1024)]
    public string? Description { get; set; }

    public SeasonEntry CurrentSeason { get; set; }
    public List<SeasonEntry> Seasons { get; set; }

    public MpmGroup(MpmUser creator, string name, string description, SeasonEntry currentSeason, List<SeasonEntry> seasons)
    {
        Creator = creator;
        Name = name;
        Description = description;
        CurrentSeason = currentSeason;
        Seasons = seasons;
        UserGroupEntries.Add(new UserGroupEntry(creator, this));
    }

    private MpmGroup() : this(null!, null!, null!, null!, null!)
    {
        
    }
}