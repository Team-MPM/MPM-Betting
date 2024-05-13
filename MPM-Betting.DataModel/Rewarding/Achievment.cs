using System.ComponentModel.DataAnnotations;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.DataModel.Rewarding;

public class Achievment
{
    [Key, StringLength(50)]
    public string Title { get; set; } = "";

    [StringLength(200)] public string Description { get; set; } = "";
    
    public List<MpmUser> Users { get; set; } = [];
}