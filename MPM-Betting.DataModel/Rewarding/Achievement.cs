using System.ComponentModel.DataAnnotations;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.DataModel.Rewarding;

public class Achievement(string title, string description)
{
    [Key] 
    public int Id { get; set; }
    
    [StringLength(50)]
    public string Title { get; set; } = title;

    [StringLength(200)] public string Description { get; set; } = description;

    public List<MpmUser> Users { get; set; } = [];
}