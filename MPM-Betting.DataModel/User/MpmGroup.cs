using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MPM_Betting.DataModel.User;

public class MpmGroup
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("ID")]
    public int Id { get; set; }
    [Column("CREATOR_ID")]
    public int CreatorId { get; set; }
    public MpmUser Creator { get; set; }
    [Column("USERS")]
    public List<UserGroupEntry> UserGroupEntries { get; set; } = [];
    
    [Required]
    [StringLength(30), Column("GROUP_NAME")]
    public string Name { get; set; } = null!;
    
    [Column("GROUP_DESCRIPTION")]
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