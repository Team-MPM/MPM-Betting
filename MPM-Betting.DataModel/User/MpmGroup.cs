using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MPM_Betting.DataModel.Betting;

namespace MPM_Betting.DataModel.User;
/// <summary>
/// Group Class
/// </summary>
public class MpmGroup
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public MpmUser Creator { get; set; }
    public List<UserGroupEntry> UserGroupEntries { get; set; } = [];
    
    [Required]
    [StringLength(30)]
    public string Name { get; set; } = null!;
    
    [StringLength(1024)]
    public string? Description { get; set; }
    [Required]
    public List<Bet> Bets { get; set; }

    [Required] public int EntryFee { get; set; } = 0;

    public int GroupLimit { get; set; } = 0;

    public MpmGroup(MpmUser creator)
    {
        Creator = creator;
        UserGroupEntries.Add(new UserGroupEntry(creator, this));
    }

    private MpmGroup() : this(null!) { }
}