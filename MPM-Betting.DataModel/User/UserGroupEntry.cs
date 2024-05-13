using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MPM_Betting.DataModel.Betting;

namespace MPM_Betting.DataModel.User;

public class UserGroupEntry(MpmUser user, MpmGroup group)
{
    [Key]
    public int Id { get; set; }
    
    public MpmUser MpmUser { get; set; } = user;
    public MpmGroup Group { get; set; } = group;

    public int Score { get; set; } = 0;
    public EGroupRole Role { get; set; }
    
    public List<ScoreEntry> ScoreEntries { get; set; } = [];
    
    private UserGroupEntry() : this(null!, null!)
    {
        
    }
}