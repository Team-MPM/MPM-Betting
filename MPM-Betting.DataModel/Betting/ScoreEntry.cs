using System.ComponentModel.DataAnnotations;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.DataModel.Betting;

public class ScoreEntry(UserGroupEntry userGroupEntry, SeasonEntry seasonEntry)
{
    [Key]
    public int Id { get; set; }
    
    public UserGroupEntry UserGroupEntry { get; set; } = userGroupEntry;
    
    public SeasonEntry SeasonEntry { get; set; } = seasonEntry;
    
    public int Score { get; set; } = 0;

    private ScoreEntry() : this(null!, null!) {}
    
}