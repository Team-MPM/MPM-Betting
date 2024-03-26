using System.ComponentModel.DataAnnotations;

namespace MPM_Betting.DataModel.User;

public class MpmGroup
{
    [Key]
    public int Id { get; set; }
    
    public MpmUser Creator { get; set; }
    public List<UserGroupEntry> UserGroupEntries { get; set; } = [];
    
    [Required]
    [StringLength(30)]
    public string Name { get; set; } = null!;

    [StringLength(1024)]
    public string? Description { get; set; }

    public MpmGroup(MpmUser creator)
    {
        Creator = creator;
        UserGroupEntries.Add(new UserGroupEntry(creator, this));
    }

    private MpmGroup() : this(null!)
    {
        
    }
}