using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MPM_Betting.DataModel.User;

public class UserGroupEntry(MpmUser user, MpmGroup group)
{
    [Key]
    public int Id { get; set; } // This sucks but ef just doesnt want the user Guid as CPK
    public MpmUser MpmUser { get; set; } = user;
    public MpmGroup Group { get; set; } = group;

    public int Score { get; set; } = 0;

    private UserGroupEntry() : this(null!, null!)
    {
        
    }
}