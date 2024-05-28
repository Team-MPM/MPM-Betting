using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MPM_Betting.DataModel.Betting;

namespace MPM_Betting.DataModel.User;

public class UserGroupEntry(string userId, MpmGroup group)
{
    [Key]
    public int Id { get; set; }
    
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string MpmUserId { get; set; } = userId;
    public MpmUser MpmUser { get; set; } = null!;
    public int GroupId { get; set; }
    public MpmGroup Group { get; set; } = group;

    public int Score { get; set; } = 0;
    
    public EGroupRole Role { get; set; } = EGroupRole.Visitor;

    [NotMapped]
    public int RoleAsInt
    {
        get => (int) Role;
        set => Role = (EGroupRole) value;
    }
    
    [NotMapped]
    public string RoleAsString
    {
        get => Role.ToString();
        set => Role = (EGroupRole) Enum.Parse(typeof(EGroupRole), value);
    }
    
    
    public List<ScoreEntry> ScoreEntries { get; set; } = [];
    
    private UserGroupEntry() : this(null!, null!) { }
}