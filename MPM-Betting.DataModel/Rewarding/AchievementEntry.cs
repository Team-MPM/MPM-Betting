using System.ComponentModel.DataAnnotations;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.DataModel.Rewarding;

public class AchievementEntry(Achievement achievement, MpmUser user)
{
    [Key]
    public int Id { get; set; }

    public Achievement Achievement { get; set; } = achievement;
    public MpmUser User { get; set; } = user;

    public DateTime DateEarned { get; set; } = DateTime.Now;
    
    private AchievementEntry () : this(null!, null!) {}
}